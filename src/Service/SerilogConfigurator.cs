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
            var loggerConfiguration = new LoggerConfiguration();

            return loggerConfiguration
                .MinimumLevel.ControlledBy(LevelSwitch)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code);
        }

        public static void SetMinimumLogLevel(string level)
        {
            LevelSwitch.MinimumLevel = level switch
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