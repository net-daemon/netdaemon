using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Exceptions;
using NetDaemon.Daemon;
using NetDaemon.Daemon.Config;
using NetDaemon.Service.App;

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

        private bool _entitiesGenerated;
        private IEnumerable<Type>? _loadedDaemonApps;

        private string? _sourcePath;

        private bool _hasConnectedBefore;

        public RunnerService(
            ILoggerFactory loggerFactory,
            IOptions<NetDaemonSettings> netDaemonSettings,
            IOptions<HomeAssistantSettings> homeAssistantSettings,
            IServiceProvider serviceProvider,
            IYamlConfig yamlConfig,
            IDaemonAppCompiler daemonAppCompiler
            )
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
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping NetDaemon service...");
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }

        [SuppressMessage("", "CA1031")]
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

                _logger.LogTrace("Finding apps in {folder}...", _sourcePath);

                // Automatically create source directories
                if (_sourcePath is string && !Directory.Exists(_sourcePath))
                {
                    _logger.LogError("Path to app source does not exist {path}, exiting...", _sourcePath);
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
                                    await GenerateEntities(daemonHost, _sourcePath).ConfigureAwait(false);

                                if (_loadedDaemonApps is null)
                                    _loadedDaemonApps = _daemonAppCompiler.GetApps();

                                if (_loadedDaemonApps?.Any() != true)
                                {
                                    _logger.LogError("No NetDaemon apps could be found, exiting...");
                                    return;
                                }

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
        private async Task GenerateEntities(NetDaemonHost daemonHost, string sourceFolder)
        {
            if (!_netDaemonSettings.GenerateEntities.GetValueOrDefault())
                return;

            if (_entitiesGenerated)
                return;

            _logger.LogTrace("Generating entities from Home Assistant instance ..");

            _entitiesGenerated = true;

            var services = await daemonHost.GetAllServices().ConfigureAwait(false);
            var sourceRx = CodeGenerator.GenerateCodeRx(
                "NetDaemon.Generated.Reactive",
                daemonHost.State.Select(n => n.EntityId).Distinct().ToList(),
                services.ToList()
            );

            await File.WriteAllTextAsync(Path.Combine(sourceFolder!, "_EntityExtensionsRx.cs.gen"), sourceRx).ConfigureAwait(false);
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
