using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service;

namespace runner
{
    internal class RunnerService : BackgroundService
    {
        private readonly NetDaemonHost _daemonHost;
        private readonly ILogger<RunnerService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        //private readonly 

        public RunnerService(ILoggerFactory loggerFactory, ILoggerFactory factory)
        {
            _logger = factory.CreateLogger<RunnerService>();
            _loggerFactory = factory;
            _daemonHost = new NetDaemonHost(new HassClient(factory), factory);
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

                var csManager = new CSScriptManager(Path.Combine(config.SourceFolder!, "apps"), _daemonHost,
                    _loggerFactory);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var hasBeenCanceledByTheDaemon = false;
                    try
                    {
                        var task = _daemonHost.Run(config.Host!, config.Port.Value!, config.Ssl.Value!, config.Token!,
                            stoppingToken);
                        await Task.Delay(500, stoppingToken); // Todo: Must be smarter later 
                        if (_daemonHost.Connected)
                        {
                            
                            await csManager.LoadSources();
                            await task;
                        }
                        else
                        {
                            _logger.LogWarning("Home assistant still unavailable, retrying in 5 seconds...");
                            hasBeenCanceledByTheDaemon = false;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            hasBeenCanceledByTheDaemon = false;
                            _logger.LogWarning("Home assistant disconnected!, retrying in 5 seconds...");
                        }
                    }

                    if (!stoppingToken.IsCancellationRequested)
                        // The service is still running, we have error in connection to hass
                        await Task.Delay(5000, stoppingToken); // Wait 5 seconds
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
        private Config ReadConfig()
        {
            try
            {
                // Check if we have HASSIO add-on options
                if (File.Exists("/data/options.json"))
                {
                    var hassioConfig = JsonSerializer.Deserialize<Config>(File.ReadAllBytes("/data/options.json"));
                    hassioConfig.Host = "hassio";
                    hassioConfig.Port = 0;
                    hassioConfig.Token = Environment.GetEnvironmentVariable("HASSIO_TOKEN");
                    return hassioConfig;
                }

                // Check if config is in a file same folder as exefile 
                var filenameForExecutingAssembly = Assembly.GetExecutingAssembly().Location;
                var folderOfExecutingAssembly = Path.GetDirectoryName(filenameForExecutingAssembly);
                var configFilePath = Path.Combine(folderOfExecutingAssembly, "config.json");

                if (File.Exists(configFilePath))
                    return JsonSerializer.Deserialize<Config>(File.ReadAllBytes(configFilePath));

                var exampleFilePath = Path.Combine(folderOfExecutingAssembly, "_config.json");
                if (!File.Exists(exampleFilePath))
                    JsonSerializer.SerializeAsync(File.OpenWrite(exampleFilePath), new Config());

                var token = Environment.GetEnvironmentVariable("HASS_TOKEN");
                if (token != null)
                {
                    var config = new Config();
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