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
        private const string _hassioConfigPath = "/data/options.json";

        // Logging switch
        private static LoggingLevelSwitch _levelSwitch = new LoggingLevelSwitch();

        public static async Task Main(string[] args)
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
}