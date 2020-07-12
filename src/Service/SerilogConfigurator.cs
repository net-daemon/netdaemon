using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Service
{
    internal static class SerilogConfigurator
    {
        private static readonly LoggingLevelSwitch LevelSwitch = new LoggingLevelSwitch();
        
        public static LoggerConfiguration Configure()
        {
            var minimumLevel = GetMinimumLogLevel();
            
            SetMinimumLogLevel(minimumLevel);

            return new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LevelSwitch)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code);
        }

        private static string GetMinimumLogLevel()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var logValue = configuration.GetSection("Logging")["MinimumLevel"];

            return string.IsNullOrEmpty(logValue) ? "info" : logValue;
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