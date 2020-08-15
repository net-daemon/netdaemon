using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetDaemon.Daemon;
using NetDaemon.Service.App;
using NetDaemon.Service.Configuration;

namespace NetDaemon.Service
{
    public class RunnerService : BackgroundService
    {
        /// <summary>
        /// The interval used when disconnected
        /// </summary>
        private const int ReconnectInterval = 10000;
        private const string Version = "custom compiled";

        private readonly HomeAssistantSettings _homeAssistantSettings;
        private readonly NetDaemonSettings _netDaemonSettings;

        private readonly ILogger<RunnerService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        private readonly IServiceProvider _serviceProvider;

        private bool _entitiesGenerated;

        public RunnerService(
            ILoggerFactory loggerFactory,
            IOptions<NetDaemonSettings> netDaemonSettings,
            IOptions<HomeAssistantSettings> homeAssistantSettings,
            IServiceProvider serviceProvider
            )
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<RunnerService>();
            _homeAssistantSettings = homeAssistantSettings.Value;
            _netDaemonSettings = netDaemonSettings.Value;
            _serviceProvider = serviceProvider;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping NetDaemon...");
            await base.StopAsync(cancellationToken).ConfigureAwait(false);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (_netDaemonSettings == null)
                {
                    _logger.LogError("No config specified, file or environment variables! Exiting...");
                    return;
                }

                EnsureApplicationDirectoryExists(_netDaemonSettings);

                var storageFolder = Path.Combine(_netDaemonSettings.SourceFolder!, ".storage");
                var sourceFolder = Path.Combine(_netDaemonSettings.SourceFolder!, "apps");

                // Automatically create source directories
                if (!Directory.Exists(sourceFolder))
                    Directory.CreateDirectory(sourceFolder);

                var hasConnectedBefore = false;

                CollectibleAssemblyLoadContext? alc = null;
                IEnumerable<Type>? loadedDaemonApps = null;

                await using var daemonHost =
                            _serviceProvider.GetService<NetDaemonHost>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (hasConnectedBefore)
                        {
                            // This is due to re-connect, it must be a re-connect
                            // so delay before retry connect again
                            await Task.Delay(ReconnectInterval, stoppingToken).ConfigureAwait(false); // Wait x seconds
                            _logger.LogInformation($"Restarting NeDaemon (version {Version})...");
                        }



                        var daemonHostTask = daemonHost.Run(
                            _homeAssistantSettings.Host,
                            _homeAssistantSettings.Port,
                            _homeAssistantSettings.Ssl,
                            _homeAssistantSettings.Token,
                            stoppingToken
                        );

                        if (await WaitForDaemonToConnect(daemonHost, stoppingToken).ConfigureAwait(false) == false)
                        {
                            continue;
                        }

                        if (!stoppingToken.IsCancellationRequested)
                        {
                            if (daemonHost.Connected)
                            {
                                try
                                {
                                    // Generate code if requested
                                    await GenerateEntities(daemonHost, sourceFolder);

                                    if (loadedDaemonApps is null)
                                    {
                                        (loadedDaemonApps, alc) = DaemonCompiler.GetDaemonApps(sourceFolder!, _logger);
                                    }

                                    if (loadedDaemonApps is null || !loadedDaemonApps.Any())
                                    {
                                        _logger.LogWarning("No .cs files files found, please add files to {sourceFolder}/apps", sourceFolder);
                                        return;
                                    }

                                    IInstanceDaemonApp? codeManager = new CodeManager(sourceFolder, loadedDaemonApps, _logger);
                                    await daemonHost.Initialize(codeManager).ConfigureAwait(false);

                                    // Wait until daemon stops
                                    await daemonHostTask.ConfigureAwait(false);
                                    if (!stoppingToken.IsCancellationRequested)
                                    {
                                        // It is disconnected, wait
                                        // _logger.LogWarning($"Home assistant is unavailable, retrying in {ReconnectInterval / 1000} seconds...");
                                    }
                                }
                                catch (TaskCanceledException)
                                {
                                    _logger.LogInformation("Canceling NetDaemon service...");
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
                        _logger.LogError(e, "MAJOR ERROR!");
                    }
                    finally
                    {
                        await daemonHost.Stop().ConfigureAwait(false);
                    }

                    // If we reached here it could be a re-connect
                    hasConnectedBefore = true;

                }
                if (alc is object)
                {
                    loadedDaemonApps = null;
                    var alcWeakRef = new WeakReference(alc, trackResurrection: true);
                    alc.Unload();
                    alc = null;

                    for (int i = 0; alcWeakRef.IsAlive && (i < 100); i++)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
            catch (OperationCanceledException)
            {
            } // Normal exit
            catch (Exception e)
            {
                _logger.LogError(e, "NetDaemon had unhandled exception, closing...");
            }

            _logger.LogInformation("NetDaemon exited!");
        }

        private async Task GenerateEntities(NetDaemonHost daemonHost, string sourceFolder)
        {
            if (!_netDaemonSettings.GenerateEntities.GetValueOrDefault())
                return;

            if (_entitiesGenerated)
                return;

            _entitiesGenerated = true;
            var codeGen = new CodeGenerator();
            var source = codeGen.GenerateCode(
                "Netdaemon.Generated.Extensions",
                daemonHost.State.Select(n => n.EntityId).Distinct()
            );

            await File.WriteAllTextAsync(Path.Combine(sourceFolder!, "_EntityExtensions.cs"), source).ConfigureAwait(false);

            var services = await daemonHost.GetAllServices();
            var sourceRx = codeGen.GenerateCodeRx(
                "Netdaemon.Generated.Reactive",
                daemonHost.State.Select(n => n.EntityId).Distinct(),
                services
            );

            await File.WriteAllTextAsync(Path.Combine(sourceFolder!, "_EntityExtensionsRx.cs"), sourceRx).ConfigureAwait(false);
        }

        private void EnsureApplicationDirectoryExists(NetDaemonSettings settings)
        {
            settings.SourceFolder ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".netdaemon");
            var appDirectory = Path.Combine(settings.SourceFolder, "apps");

            Directory.CreateDirectory(appDirectory);
        }

        private async Task<bool> WaitForDaemonToConnect(NetDaemonHost daemonHost, CancellationToken stoppingToken)
        {
            var nrOfTimesCheckForConnectedState = 0;

            while (!daemonHost.Connected && !stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(500, stoppingToken).ConfigureAwait(false);
                if (nrOfTimesCheckForConnectedState++ > 5)
                    break;
            }
            return daemonHost.Connected;
        }
    }
}