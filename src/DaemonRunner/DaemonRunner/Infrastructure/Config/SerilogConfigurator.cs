using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetDaemon.Common.Exceptions;
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
            _ = loggerConfiguration ??
               throw new NetDaemonArgumentNullException(nameof(loggerConfiguration));
            _ = hostingEnvironment ??
               throw new NetDaemonArgumentNullException(nameof(hostingEnvironment));

            var loggingConfiguration = GetLoggingConfiguration(hostingEnvironment);
            SetMinimumLogLevel(loggingConfiguration.MinimumLevel);

            return loggerConfiguration
                .MinimumLevel.ControlledBy(LevelSwitch)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: NetDaemonConsoleThemes.GetThemeByType(loggingConfiguration.ConsoleThemeType), applyThemeToRedirectedOutput: true,
                                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext:l}: {Message:lj}{NewLine}{Exception}");
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
            _ = level ??
               throw new NetDaemonArgumentNullException(nameof(level));
            LevelSwitch.MinimumLevel = level.ToLower(CultureInfo.InvariantCulture) switch
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