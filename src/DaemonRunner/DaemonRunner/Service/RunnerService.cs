using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service
{
    public class RunnerService : BackgroundService
    {
        private readonly NetDaemonHost _daemonHost;
        private readonly ILogger<RunnerService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDaemonAppConfig _daemonAppConfig;

        //private readonly

        public RunnerService(ILoggerFactory loggerFactory, IDaemonAppConfig? daemonAppConfig = null)
        {


            _logger = loggerFactory.CreateLogger<RunnerService>();
            _loggerFactory = loggerFactory;
            _daemonHost = new NetDaemonHost(new HassClient(loggerFactory), loggerFactory);
            daemonAppConfig ??= new DaemonAppConfig(_daemonHost, _logger);
            _daemonAppConfig = daemonAppConfig;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping netdaemon...");
            await _daemonHost.Stop();
            if (_daemonHost != null)
                await Task.WhenAny(_daemonHost.Stop(), Task.Delay(1000, cancellationToken));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting netdaemon...");

                var config = ReadConfig();
                if (config == null)
                {
                    _logger.LogError("No config specified, file or environment variables! Exiting...");

                    return;
                }
                var sourceFolder = config.SourceFolder;

                if (!string.IsNullOrEmpty(sourceFolder))
                    sourceFolder = Path.Combine(config.SourceFolder!, "apps");

                var csManager = new CSScriptManager("/workspaces/netdaemon/tests/NetDaemon.Daemon.Tests/DaemonRunner/Fixtures", _daemonHost,
                    _loggerFactory); //sourceFolder


                while (!stoppingToken.IsCancellationRequested)
                {
                    var hasBeenCanceledByTheDaemon = false;
                    try
                    {
                        var daemonHostTask = _daemonHost.Run(config.Host!, config.Port.Value!, config.Ssl.Value!, config.Token!,
                            stoppingToken);

                        var nrOfTimesCheckForConnectedState = 0;
                        while (!_daemonHost.Connected && !stoppingToken.IsCancellationRequested)
                        {
                            await Task.Delay(1000, stoppingToken);
                            if (nrOfTimesCheckForConnectedState++ > 3)
                                break;
                        }
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            if (_daemonHost.Connected)
                            {

                                await csManager.LoadSources(_daemonAppConfig);
                                await daemonHostTask;
                            }
                            else
                            {
                                _logger.LogWarning("Home assistant still unavailable, retrying in 20 seconds...");
                                hasBeenCanceledByTheDaemon = false;
                            }
                        }

                    }
                    catch (OperationCanceledException)
                    {
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            hasBeenCanceledByTheDaemon = false;
                            _logger.LogWarning("Home assistant disconnected!, retrying in 20 seconds...");
                        }
                    }

                    if (!stoppingToken.IsCancellationRequested)
                        // The service is still running, we have error in connection to hass
                        await Task.Delay(20000, stoppingToken); // Wait 5 seconds
                }
            }
            catch (OperationCanceledException)
            {
            } // Normal exit
            catch (Exception e)
            {
                _logger.LogError(e, "Some problem!");
            }

            _logger.LogInformation("Ending stuff!");
        }
        private HostConfig ReadConfig()
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
                    hassioConfig.Token = Environment.GetEnvironmentVariable("HASSIO_TOKEN");
                    hassioConfig.SourceFolder = Environment.GetEnvironmentVariable("HASS_DAEMONAPPFOLDER");
                    return hassioConfig;
                }

                // Check if config is in a file same folder as exefile
                var filenameForExecutingAssembly = Assembly.GetExecutingAssembly().Location;
                var folderOfExecutingAssembly = Path.GetDirectoryName(filenameForExecutingAssembly);
                var configFilePath = Path.Combine(folderOfExecutingAssembly, "config.json");

                if (File.Exists(configFilePath))
                    return JsonSerializer.Deserialize<HostConfig>(File.ReadAllBytes(configFilePath));

                var exampleFilePath = Path.Combine(folderOfExecutingAssembly, "_config.json");
                if (!File.Exists(exampleFilePath))
                    JsonSerializer.SerializeAsync(File.OpenWrite(exampleFilePath), new HostConfig());

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
                                          Path.Combine(folderOfExecutingAssembly, "daemonapp");
                    return config;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get configuration!");
            }

            return null;
        }
    }
}