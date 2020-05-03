using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.Extensions.DependencyInjection;

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service
{
    public static class Runner
    {
        private const string _hassioConfigPath = "/data/options.json";
        private static LoggingLevelSwitch _levelSwitch = new LoggingLevelSwitch();

        public static async Task Run(string[] args)
        {
            try
            {
                // Setup serilog
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .MinimumLevel.ControlledBy(_levelSwitch)
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    .CreateLogger();

                if (File.Exists(_hassioConfigPath))
                {
                    try
                    {
                        var hassAddOnSettings = await JsonSerializer.DeserializeAsync<HassioConfig>(
                                                      File.OpenRead(_hassioConfigPath)).ConfigureAwait(false);
                        if (hassAddOnSettings.LogLevel is object)
                        {
                            _levelSwitch.MinimumLevel = hassAddOnSettings.LogLevel switch
                            {
                                "info" => LogEventLevel.Information,
                                "debug" => LogEventLevel.Debug,
                                "error" => LogEventLevel.Error,
                                "warning" => LogEventLevel.Warning,
                                "trace" => LogEventLevel.Verbose,
                                _ => LogEventLevel.Information
                            };
                        }
                        if (hassAddOnSettings.GenerateEntitiesOnStart is object)
                        {
                            Environment.SetEnvironmentVariable("HASS_GEN_ENTITIES", hassAddOnSettings.GenerateEntitiesOnStart.ToString());
                        }
                        // We are in Hassio so hard code the path
                        Environment.SetEnvironmentVariable("HASS_DAEMONAPPFOLDER", "/config/netdaemon");
                    }
                    catch (System.Exception e)
                    {
                        Log.Fatal(e, "Failed to read the Home Assistant Add-on config");
                    }
                }
                else
                {
                    var envLogLevel = Environment.GetEnvironmentVariable("HASS_LOG_LEVEL");
                    _levelSwitch.MinimumLevel = envLogLevel switch
                    {
                        "info" => LogEventLevel.Information,
                        "debug" => LogEventLevel.Debug,
                        "error" => LogEventLevel.Error,
                        "warning" => LogEventLevel.Warning,
                        "trace" => LogEventLevel.Verbose,
                        _ => LogEventLevel.Information
                    };
                }

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Failed to start host...");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices(services => { services.AddHostedService<RunnerService>(); });
    }

    public class RunnerService : BackgroundService
    {


        const string _version = "dev";

        // private NetDaemonHost? _daemonHost;
        private readonly ILogger<RunnerService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// The intervall used when disconnected
        /// </summary>
        private const int _reconnectIntervall = 40000;

        public RunnerService(ILoggerFactory loggerFactory)
        {
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
                _logger.LogInformation($"Starting NeDaemon (version {_version})...");

                var config = await ReadConfigAsync().ConfigureAwait(false);

                if (config == null)
                {
                    _logger.LogError("No config specified, file or environment variables! Exiting...");

                    return;
                }

                EnsureApplicationDirectoryExists(config);

                var sourceFolder = config.SourceFolder;
                var storageFolder = Path.Combine(config.SourceFolder!, ".storage");

                sourceFolder = Path.Combine(config.SourceFolder!, "apps");

                // Automatically create source directories
                if (!System.IO.Directory.Exists(sourceFolder))
                    System.IO.Directory.CreateDirectory(sourceFolder);

                bool hasConnectedBefore = false;
                bool generatedEntities = false;
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (hasConnectedBefore)
                        {
                            // This is due to re-connect, it must be a re-connect
                            // so delay before retry connect again
                            await Task.Delay(_reconnectIntervall, stoppingToken).ConfigureAwait(false); // Wait x seconds
                        }

                        await using var _daemonHost = new NetDaemonHost(new HassClient(_loggerFactory), new DataRepository(storageFolder), _loggerFactory);

                        var daemonHostTask = _daemonHost.Run(config.Host, config.Port, config.Ssl, config.Token,
                            stoppingToken);

                        await WaitForDaemonToConnect(_daemonHost, stoppingToken).ConfigureAwait(false);

                        if (!stoppingToken.IsCancellationRequested)
                        {
                            if (_daemonHost.Connected)
                            {
                                try
                                {
                                    // Generate code if requested
                                    var envGenEntities = Environment.GetEnvironmentVariable("HASS_GEN_ENTITIES") ?? config.GenerateEntitiesOnStartup?.ToString();
                                    if (envGenEntities is object)
                                    {
                                        if (envGenEntities == "True" && !generatedEntities)
                                        {
                                            generatedEntities = true;
                                            var codeGen = new CodeGenerator();
                                            var source = codeGen.GenerateCode("Netdaemon.Generated.Extensions",
                                                _daemonHost.State.Select(n => n.EntityId).Distinct());

                                            System.IO.File.WriteAllText(System.IO.Path.Combine(sourceFolder!, "_EntityExtensions.cs"), source);
                                        }
                                    }
                                    using (var codeManager = new CodeManager(sourceFolder!, _daemonHost.Logger))
                                    {
                                        await codeManager.EnableApplicationDiscoveryServiceAsync(_daemonHost, discoverServicesOnStartup: true).ConfigureAwait(false);

                                        // Wait until daemon stops
                                        await daemonHostTask.ConfigureAwait(false);
                                        if (!stoppingToken.IsCancellationRequested)
                                        {
                                            // It is disconnet, wait
                                            _logger.LogWarning($"Home assistant is unavailable, retrying in {_reconnectIntervall / 1000} seconds...");
                                        }
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
                    catch (OperationCanceledException)
                    {
                        if (!stoppingToken.IsCancellationRequested)
                        {

                            _logger.LogWarning($"Home assistant is disconnected, retrying in {_reconnectIntervall / 1000} seconds...");
                        }
                    }
                    // If we reached here it could be a re-connect
                    hasConnectedBefore = true;
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

        private async Task<HostConfig?> ReadConfigAsync()
        {
            try
            {
                // Check if we have HASSIO add-on options

                // if (File.Exists("/data/options.json"))    Todo: We read configs here later
                if (Environment.GetEnvironmentVariable("HASSIO_TOKEN") != null)
                {
                    //var hassioConfig = JsonSerializer.Deserialize<Config>(File.ReadAllBytes("/data/options.json"));
                    var hassioConfig = new HostConfig();
                    hassioConfig.Host = "";
                    hassioConfig.Port = 0;
                    hassioConfig.Token = Environment.GetEnvironmentVariable("HASSIO_TOKEN") ?? string.Empty;
                    hassioConfig.SourceFolder = Environment.GetEnvironmentVariable("HASS_DAEMONAPPFOLDER");
                    return hassioConfig;
                }

                // Check if config is in a file same folder as exefile
                var filenameForExecutingAssembly = Assembly.GetExecutingAssembly().Location;
                var folderOfExecutingAssembly = Path.GetDirectoryName(filenameForExecutingAssembly);
                var configFilePath = Path.Combine(folderOfExecutingAssembly!, "daemon_config.json");

                if (File.Exists(configFilePath))
                    return JsonSerializer.Deserialize<HostConfig>(File.ReadAllBytes(configFilePath));

                var token = Environment.GetEnvironmentVariable("HASS_TOKEN");
                if (token != null)
                {
                    var config = new HostConfig();
                    config.Token = token;
                    config.Host = Environment.GetEnvironmentVariable("HASS_HOST") ?? config.Host;
                    config.Port = short.TryParse(Environment.GetEnvironmentVariable("HASS_PORT"), out var port)
                        ? port
                        : config.Port;
                    config.SourceFolder = Environment.GetEnvironmentVariable("HASS_DAEMONAPPFOLDER") ??
                                          Path.Combine(folderOfExecutingAssembly!, "daemonapp");
                    return config;
                }

                var exampleFilePath = Path.Combine(folderOfExecutingAssembly!, "daemon_config_example.json");
                if (!File.Exists(exampleFilePath))
                {
                    var json = JsonSerializer.Serialize(new HostConfig());

                    using (var fileStream = new FileStream(exampleFilePath, FileMode.CreateNew))
                    {
                        var options = new JsonSerializerOptions { WriteIndented = true, IgnoreNullValues = true };
                        await JsonSerializer.SerializeAsync(fileStream, new HostConfig(), options).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get configuration!");
            }

            return null;
        }

        private void EnsureApplicationDirectoryExists(HostConfig config)
        {
            config.SourceFolder ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".netdaemon");
            var appDirectory = Path.Combine(config.SourceFolder, "apps");

            Directory.CreateDirectory(appDirectory);
        }
    }
}