using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Exceptions;
using NetDaemon.Daemon;
using NetDaemon.Daemon.Config;
using NetDaemon.Service.App;
using NetDaemon.Service.App.CodeGeneration;

namespace NetDaemon.Service
{
    public class RunnerService : BackgroundService
    {
        /// <summary>
        /// The interval used when disconnected
        /// </summary>
        private const int ReconnectInterval = 30000;
        private const string Version = "custom compiled";

        private readonly HomeAssistantSettings _homeAssistantSettings;
        private readonly NetDaemonSettings _netDaemonSettings;

        private readonly ILogger<RunnerService> _logger;

        private readonly IServiceProvider _serviceProvider;
        private readonly IYamlConfig _yamlConfig;
        private readonly IDaemonAppCompiler _daemonAppCompiler;
        private readonly ICodeGenerationHandler _codeGenerationHandler;

        private bool _entitiesGenerated;
        private IEnumerable<Type>? _loadedDaemonApps;
        private IEnumerable<Assembly>? _loadedDaemonAssemblies;

        private string? _sourcePath;

        private bool _hasConnectedBefore;
        private readonly IWebHostEnvironment _environment;

        public RunnerService(
            ILoggerFactory loggerFactory,
            IOptions<NetDaemonSettings> netDaemonSettings,
            IOptions<HomeAssistantSettings> homeAssistantSettings,
            IServiceProvider serviceProvider,
            IYamlConfig yamlConfig,
            IDaemonAppCompiler daemonAppCompiler,
            INetDaemonAssemblies daemonAssemblies,
            ICodeGenerationHandler codeGenerationHandler,
            IWebHostEnvironment environment)
        {
            _ = homeAssistantSettings ??
                throw new NetDaemonArgumentNullException(nameof(homeAssistantSettings));
            _ = netDaemonSettings ??
               throw new NetDaemonArgumentNullException(nameof(netDaemonSettings));
            _logger = loggerFactory.CreateLogger<RunnerService>();
            _homeAssistantSettings = homeAssistantSettings.Value;
            _netDaemonSettings = netDaemonSettings.Value;
            _serviceProvider = serviceProvider;
            _yamlConfig = yamlConfig;
            _daemonAppCompiler = daemonAppCompiler;
            _codeGenerationHandler = codeGenerationHandler;
            _environment = environment;
            _loadedDaemonAssemblies = daemonAssemblies.LoadedAssemblies;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping NetDaemon service...");
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        [SuppressMessage("", "CA1031")]
        [SuppressMessage("", "CA2007")]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation($"Starting NeDaemon service (version {Version})...");

                if (_netDaemonSettings == null)
                {
                    _logger.LogError("No configuration specified, in appsettings or environment variables! Exiting...");
                    return;
                }

                _sourcePath = _netDaemonSettings.GetAppSourceDirectory();

                _logger.LogTrace("Finding apps in {Folder}...", _sourcePath);

                // Automatically create source directories
                if (string.IsNullOrEmpty(_sourcePath))
                {
                    _logger.LogError("Path to folder cannot be null, exiting...", _sourcePath);
                    return;
                }
                if (!Directory.Exists(_sourcePath))
                {
                    _logger.LogError("Path to app source does not exist {Path}, exiting...", _sourcePath);
                    return;
                }

                _loadedDaemonApps = null;

                await using var daemonHost = _serviceProvider.GetService<NetDaemonHost>()
                    ?? throw new NetDaemonException("Failed to get service for NetDaemonHost");
                {
                    await Run(daemonHost, stoppingToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            } // Normal exit
            catch (Exception e)
            {
                _logger.LogError(e, "NetDaemon had unhandled exception, closing...");
            }

            _logger.LogInformation("NetDaemon service exited!");
        }

        [SuppressMessage("", "CA1031")]
        private async Task Run(NetDaemonHost daemonHost, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_hasConnectedBefore)
                    {
                        // This is due to re-connect, it must be a re-connect
                        // so delay before retry connect again
                        await Task.Delay(ReconnectInterval, stoppingToken).ConfigureAwait(false); // Wait x seconds
                        _logger.LogInformation($"Restarting NeDaemon service (version {Version})...");
                    }

                    var daemonHostTask = daemonHost.Run(
                        _homeAssistantSettings.Host,
                        _homeAssistantSettings.Port,
                        _homeAssistantSettings.Ssl,
                        _homeAssistantSettings.Token,
                        stoppingToken
                    );

                    if (!await WaitForDaemonToConnect(daemonHost, stoppingToken).ConfigureAwait(false))
                    {
                        continue;
                    }

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        if (daemonHost.IsConnected)
                        {
                            try
                            {
                                // Generate code if requested
                                if (_sourcePath is string)
                                    await GenerateEntitiesAsync(daemonHost, _sourcePath).ConfigureAwait(false);

                                if (_loadedDaemonApps is null)
                                    _loadedDaemonApps = _daemonAppCompiler.GetApps(_loadedDaemonAssemblies);

                                if (_loadedDaemonApps?.Any() != true)
                                {
                                    _logger.LogError("No NetDaemon apps could be found, exiting...");
                                    return;
                                }

                                _loadedDaemonApps = FilterFocusApps(_loadedDaemonApps.ToList());

                                await HassModel.DependencyInjectionSetup.InitializeAsync(_serviceProvider, stoppingToken).ConfigureAwait(false);

                                IInstanceDaemonApp? codeManager = new CodeManager(_loadedDaemonApps, _logger, _yamlConfig);
                                await daemonHost.Initialize(codeManager).ConfigureAwait(false);

                                // Wait until daemon stops
                                await daemonHostTask.ConfigureAwait(false);
                            }
                            catch (TaskCanceledException)
                            {
                                _logger.LogTrace("Canceling NetDaemon service...");
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Failed to load applications");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Home Assistant Core still unavailable, retrying in {ReconnectInterval / 1000} seconds...");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning($"Home assistant is disconnected, retrying in {ReconnectInterval / 1000} seconds...");
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("Error in NetDaemon service!, set trace log level to see details.");
                    _logger.LogTrace(e, "Error in NetDaemon service!");
                }
                finally
                {
                    try
                    {
                        await daemonHost.Stop().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Error stopping NetDaemonInstance, enable trace level logging for details");
                        _logger.LogTrace(e, "Error stopping NetDaemonInstance");
                    }
                }

                // If we reached here it could be a re-connect
                _hasConnectedBefore = true;
            }
        }

        private IEnumerable<Type> FilterFocusApps(IReadOnlyCollection<Type> allApps)
        {
            var focusApps = allApps.Where(a => a.GetCustomAttribute<FocusAttribute>() != null).ToList();

            if (focusApps.Count == 0) return allApps;

            foreach (var focusApp in focusApps)
            {
                _logger.LogInformation("[Focus] attribute is set for app {AppName}", focusApp.FullName);
            }

            if (!_environment.IsDevelopment())
            {
                _logger.LogError("{Count} Focus apps were found but current environment is not 'Development', the [Focus] attribute is ignored" +
                                 "Make sure the environment variable `DOTNET_ENVIRONMENT` is set to `Development` to use [Focus] or remove the [Focus] attribute when running in production", focusApps.Count);
                return allApps;
            }
            
            _logger.LogWarning($"Found {focusApps.Count} [Focus] apps, skipping all other apps");
            return focusApps;
        }

        public async Task GenerateEntitiesAsync(NetDaemonHost daemonHost, string sourceFolder)
        {
            if (!_netDaemonSettings.GenerateEntities.GetValueOrDefault())
                return;

            if (_entitiesGenerated)
                return;

            _logger.LogTrace("Generating entities from Home Assistant instance ..");

            _entitiesGenerated = true;

            await _codeGenerationHandler.GenerateEntitiesAsync(daemonHost, sourceFolder).ConfigureAwait(false);
        }

        private static async Task<bool> WaitForDaemonToConnect(NetDaemonHost daemonHost, CancellationToken stoppingToken)
        {
            var nrOfTimesCheckForConnectedState = 0;

            while (!daemonHost.IsConnected && !stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(500, stoppingToken).ConfigureAwait(false);
                if (nrOfTimesCheckForConnectedState++ > 5)
                    break;
            }
            return daemonHost.IsConnected;
        }
    }
}