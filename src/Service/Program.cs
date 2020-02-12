using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service
{

    class Program
    {
        private const string _hassioConfigPath = "data/options.json";
        private static LogLevel LogLevel = LogLevel.Information;
        public static async Task Main(string[] args)
        {

            try
            {
                ///
                if (File.Exists(_hassioConfigPath))
                {

                    var hassAddOnSettings = await JsonSerializer.DeserializeAsync<System.Collections.Generic.Dictionary<string, string>>(
                                                        File.OpenRead(_hassioConfigPath));
                    if (hassAddOnSettings.ContainsKey("log_level"))
                    {
                        Program.LogLevel = hassAddOnSettings["log_level"] switch
                        {
                            "information" => LogLevel.Information,
                            "debug" => LogLevel.Debug,
                            "error" => LogLevel.Error,
                            "warning" => LogLevel.Warning,
                            "trace" => LogLevel.Trace,
                            _ => LogLevel.Information
                        };
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
