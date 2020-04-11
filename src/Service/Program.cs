using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Service
{
    internal class Program
    {
        // private const string _hassioConfigPath = "/root/src/src/Service/.config/hassio_config.json";
        private const string _hassioConfigPath = "data/options.json";
        private static LogEventLevel LogLevel = LogEventLevel.Verbose;

        public static async Task Main(string[] args)
        {
            try
            {


                if (File.Exists(_hassioConfigPath))
                {
                    var hassAddOnSettings = await JsonSerializer.DeserializeAsync<HassioConfig>(
                                                        File.OpenRead(_hassioConfigPath)).ConfigureAwait(false);
                    if (hassAddOnSettings.LogLevel is object)
                    {
                        Program.LogLevel = hassAddOnSettings.LogLevel switch
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
                }
                else
                {
                    var envLogLevel = Environment.GetEnvironmentVariable("HASS_LOG_LEVEL");
                    Program.LogLevel = envLogLevel switch
                    {
                        "info" => LogEventLevel.Information,
                        "debug" => LogEventLevel.Debug,
                        "error" => LogEventLevel.Error,
                        "warning" => LogEventLevel.Warning,
                        "trace" => LogEventLevel.Verbose,
                        _ => LogEventLevel.Information
                    };
                }

                // Setup serilog
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .MinimumLevel.Is(Program.LogLevel)
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    .CreateLogger();

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Home assistant add-on config not valid json, ending add-on...");
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
        // .ConfigureLogging(logging =>
        // {
        //     logging.ClearProviders();
        //     // logging.AddConsole(options => options.IncludeScopes = false);
        //     // logging.AddDebug();
        //     // logging.AddFilter("Microsoft", LogLevel.Error);
        //     logging.AddSerilog();
        // }
        // );
    }
}