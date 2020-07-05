using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace NetDaemon.Service.Support
{
    internal static class SeriLogConfigurator
    {
        private static readonly LoggingLevelSwitch LevelSwitch = new LoggingLevelSwitch();

        public static LoggerConfiguration GetConfiguration()
        {
            return Configure(new LoggerConfiguration());
        }
        
        public static LoggerConfiguration Configure(LoggerConfiguration configuration)
        {
            return configuration
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