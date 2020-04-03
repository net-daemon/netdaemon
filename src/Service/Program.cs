using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service
{
    internal class Program
    {
        // private const string _hassioConfigPath = "/root/src/src/Service/.config/hassio_config.json";
        private const string _hassioConfigPath = "data/options.json";
        private static LogLevel LogLevel = LogLevel.Trace;

        public static async Task Main(string[] args)
        {
            try
            {
                ///
                if (File.Exists(_hassioConfigPath))
                {
                    var hassAddOnSettings = await JsonSerializer.DeserializeAsync<HassioConfig>(
                                                        File.OpenRead(_hassioConfigPath)).ConfigureAwait(false);
                    if (hassAddOnSettings.LogLevel is object)
                    {
                        Program.LogLevel = hassAddOnSettings.LogLevel switch
                        {
                            "info" => LogLevel.Information,
                            "debug" => LogLevel.Debug,
                            "error" => LogLevel.Error,
                            "warning" => LogLevel.Warning,
                            "trace" => LogLevel.Trace,
                            _ => LogLevel.Information
                        };
                    }
                    if (hassAddOnSettings.GenerateEntitiesOnStart is object)
                    {
                        Environment.SetEnvironmentVariable("HASS_GEN_ENTITIES", hassAddOnSettings.GenerateEntitiesOnStart.ToString());
                    }
                }
            }
            catch (Exception)
            {
                System.Console.WriteLine("Home assistant add-on config not valid json, ending add-on...");
                return;
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services => { services.AddHostedService<RunnerService>(); })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole(options => options.IncludeScopes = false);
                    logging.AddDebug();
                    logging.AddFilter("Microsoft", LogLevel.Error);
                    logging.SetMinimumLevel(Program.LogLevel);
                });
    }
}