using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetDaemon.Daemon;
using NetDaemon.Daemon.Storage;
using NetDaemon.Service.App;

namespace NetDaemon.Service
{
    public class RunnerService : BackgroundService
    {
        /// <summary>
        /// The intervall used when disconnected
        /// </summary>
        private const int _reconnectIntervall = 40000;

        private const string _version = "dev";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HostConfig _configuration;

        private readonly ILogger<RunnerService> _logger;

        private readonly ILoggerFactory _loggerFactory;
        public RunnerService(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, HostConfig configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<RunnerService>();
            _loggerFactory = loggerFactory;
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
                if (_configuration == null)
                {
                    _logger.LogError("No config specified, file or environment variables! Exiting...");
                    return;
                }

                EnsureApplicationDirectoryExists(_configuration);

                var sourceFolder = _configuration.SourceFolder;
                var storageFolder = Path.Combine(_configuration.SourceFolder!, ".storage");

                sourceFolder = Path.Combine(_configuration.SourceFolder!, "apps");

                // Automatically create source directories
                if (!Directory.Exists(sourceFolder))
                    Directory.CreateDirectory(sourceFolder);

                bool hasConnectedBefore = false;
                bool generatedEntities = false;

                CollectibleAssemblyLoadContext? alc = null;
                IEnumerable<Type>? loadedDaemonApps = null;

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (hasConnectedBefore)
                        {
                            // This is due to re-connect, it must be a re-connect
                            // so delay before retry connect again
                            await Task.Delay(_reconnectIntervall, stoppingToken).ConfigureAwait(false); // Wait x seconds
                            _logger.LogInformation($"Restarting NeDaemon (version {_version})...");
                        }

                        await using var _daemonHost =
                            new NetDaemonHost(
                                new HassClient(_loggerFactory),
                                new DataRepository(storageFolder),
                                _loggerFactory,
                                new HttpHandler(_httpClientFactory)
                                );
                        {

                            var daemonHostTask = _daemonHost.Run(_configuration.Host, _configuration.Port, _configuration.Ssl, _configuration.Token,
                                stoppingToken);

                            await WaitForDaemonToConnect(_daemonHost, stoppingToken).ConfigureAwait(false);

                            if (!stoppingToken.IsCancellationRequested)
                            {
                                if (_daemonHost.Connected)
                                {
                                    try
                                    {
                                        // Generate code if requested
                                        var envGenEntities = Environment.GetEnvironmentVariable("HASS_GEN_ENTITIES") ?? _configuration.GenerateEntitiesOnStartup?.ToString();
                                        if (envGenEntities is object)
                                        {
                                            if (envGenEntities == "True" && !generatedEntities)
                                            {
                                                generatedEntities = true;
                                                var codeGen = new CodeGenerator();
                                                var source = codeGen.GenerateCode("Netdaemon.Generated.Extensions",
                                                    _daemonHost.State.Select(n => n.EntityId).Distinct());

                                                System.IO.File.WriteAllText(System.IO.Path.Combine(sourceFolder!, "_EntityExtensions.cs"), source);

                                                var services = await _daemonHost.GetAllServices();
                                                var sourceRx = codeGen.GenerateCodeRx("Netdaemon.Generated.Reactive",
                                                    _daemonHost.State.Select(n => n.EntityId).Distinct(), services);

                                                System.IO.File.WriteAllText(System.IO.Path.Combine(sourceFolder!, "_EntityExtensionsRx.cs"), sourceRx);
                                            }
                                        }
                                        if (loadedDaemonApps is null)
                                        {
                                            (loadedDaemonApps, alc) = DaemonCompiler.GetDaemonApps(sourceFolder!, _logger);
                                        }

                                        if (loadedDaemonApps is null || loadedDaemonApps.Count() == 0)
                                        {
                                            _logger.LogWarning("No .cs files files found, please add files to [netdaemonfolder]/apps");
                                            return;
                                        }

                                        IInstanceDaemonApp? codeManager = new CodeManager(sourceFolder, loadedDaemonApps, _logger);
                                        await _daemonHost.Initialize(codeManager).ConfigureAwait(false);

                                        // Wait until daemon stops
                                        await daemonHostTask.ConfigureAwait(false);
                                        if (!stoppingToken.IsCancellationRequested)
                                        {
                                            // It is disconnected, wait
                                            _logger.LogWarning($"Home assistant is unavailable, retrying in {_reconnectIntervall / 1000} seconds...");
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
                                    _logger.LogWarning($"Home Assistant Core still unavailable, retrying in {_reconnectIntervall / 1000} seconds...");
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            _logger.LogWarning($"Home assistant is disconnected, retrying in {_reconnectIntervall / 1000} seconds...");
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "MAJOR ERROR!");
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

            _logger.LogInformation("Netdaemon exited!");
        }

        private void EnsureApplicationDirectoryExists(HostConfig config)
        {
            config.SourceFolder ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".netdaemon");
            var appDirectory = Path.Combine(config.SourceFolder, "apps");

            Directory.CreateDirectory(appDirectory);
        }

        private async Task WaitForDaemonToConnect(NetDaemonHost daemonHost, CancellationToken stoppingToken)
        {
            var nrOfTimesCheckForConnectedState = 0;

            while (!daemonHost.Connected && !stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
                if (nrOfTimesCheckForConnectedState++ > 5)
                    break;
            }
        }
    }
}