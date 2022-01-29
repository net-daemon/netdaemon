using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace NetDaemon.Extensions.Logging.Internal;

internal static class SerilogConfigurator
{
    private static readonly LoggingLevelSwitch LevelSwitch = new();

    public static LoggerConfiguration Configure(LoggerConfiguration loggerConfiguration,
        IHostEnvironment hostingEnvironment)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);
        ArgumentNullException.ThrowIfNull(hostingEnvironment);

        var loggingConfiguration = GetLoggingConfiguration(hostingEnvironment);
        SetMinimumLogLevel(loggingConfiguration.LogLevel);

        return loggerConfiguration
            .MinimumLevel.ControlledBy(LevelSwitch)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                theme: NetDaemonLoggingThemes.NetDaemonConsoleThemes.GetThemeByType(loggingConfiguration
                    .ConsoleThemeType),
                applyThemeToRedirectedOutput: true,
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext:l}: {Message:lj}{NewLine}{Exception}");
    }

    private static LoggingConfiguration GetLoggingConfiguration(IHostEnvironment hostingEnvironment)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true)
            .AddEnvironmentVariables()
            .Build();

        var loggingConfiguration = new LoggingConfiguration();
        configuration.GetSection("Logging").Bind(loggingConfiguration);
        return loggingConfiguration;
    }

    private static void SetMinimumLogLevel(LogLevel? level)
    {
        LevelSwitch.MinimumLevel = level?.Default.ToLower(CultureInfo.InvariantCulture) switch
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
