using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace NetDaemon.Infrastructure.Config
{
    public static class SerilogConfigurator
    {
        private static readonly LoggingLevelSwitch LevelSwitch = new();

        public static LoggerConfiguration Configure(LoggerConfiguration loggerConfiguration, IHostEnvironment hostingEnvironment)
        {
            var loggingConfiguration = GetLoggingConfiguration(hostingEnvironment);

            SetMinimumLogLevel(loggingConfiguration.MinimumLevel);

            return loggerConfiguration
                .MinimumLevel.ControlledBy(LevelSwitch)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: NetDaemonConsoleThemes.GetThemeByType(loggingConfiguration.ConsoleThemeType), applyThemeToRedirectedOutput: true);
        }

        private static LoggingConfiguration GetLoggingConfiguration(IHostEnvironment hostingEnvironment)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var loggingConfiguration = new LoggingConfiguration();
            configuration.GetSection("Logging").Bind(loggingConfiguration);
            return loggingConfiguration;
        }

        public static void SetMinimumLogLevel(string level)
        {
            LevelSwitch.MinimumLevel = level.ToLower() switch
            {
                "info" => LogEventLevel.Information,
                "debug" => LogEventLevel.Debug,
                "error" => LogEventLevel.Error,
                "warning" => LogEventLevel.Warning,
                "trace" => LogEventLevel.Verbose,
                _ => LogEventLevel.Information
            };
        }
    }
}