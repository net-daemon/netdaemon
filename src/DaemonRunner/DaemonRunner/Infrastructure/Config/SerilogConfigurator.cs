using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace NetDaemon.Infrastructure.Config
{
    internal class NetDaemonTheme : Serilog.Sinks.SystemConsole.Themes.ConsoleTheme
    {
        public override bool CanBuffer => false;

        protected override int ResetCharCount => 0;

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Reset(TextWriter output)
        {
            output.Write("\u001b[0m");
        }

        public override int Set(TextWriter output, ConsoleThemeStyle style)
        {
            string? x = style switch
            {
                ConsoleThemeStyle.LevelError => "\u001b[0;31m",
                ConsoleThemeStyle.Name => "\u001b[1;34m",
                ConsoleThemeStyle.LevelInformation => "\u001b[0;36m",
                ConsoleThemeStyle.LevelWarning => "\u001b[1;33m",
                ConsoleThemeStyle.LevelFatal => "\u001b[0;31m",
                ConsoleThemeStyle.LevelDebug => "\u001b[0;37m",
                ConsoleThemeStyle.Scalar => "\u001b[1;34m",
                ConsoleThemeStyle.String => "\u001b[0;36m",
                _ => null
            };

            if (x is not null)
            {
                output.Write(x);
                return x.Length;
            }
            return 0;
        }

        public override string? ToString()
        {
            return base.ToString();
        }
    }

    public static class SerilogConfigurator
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
                .WriteTo.Console(theme: new NetDaemonTheme(), applyThemeToRedirectedOutput: true);
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