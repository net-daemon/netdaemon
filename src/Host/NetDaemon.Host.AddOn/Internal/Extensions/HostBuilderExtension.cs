using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Extensions.Tts;
using NetDaemon.Host.AddOn.Internal.Config;

namespace NetDaemon.Runtime.Internal.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseNetDaemonAddon(this IHostBuilder hostBuilder)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder);

        return hostBuilder
            .UseNetDaemonAddOnSettings()
            .UseNetDaemonDefaultLogging()
            .UseNetDaemonRuntime()
            .UseNetDaemonTextToSpeech()
            .ConfigureServices((_, services) =>
            {
                services
                    .AddAppsFromSource()
                    .AddNetDaemonScheduler()
                    .AddNetDaemonStateManager();
            });
    }

    private static IHostBuilder UseNetDaemonAddOnSettings(this IHostBuilder hostBuilder)
    {
        var logLevel = LogLevel.Information;
        
        return hostBuilder
            .ConfigureServices((_, services) => { services.AddNetDaemonAddOnConfiguration(); })
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json");

                var addOnConfig = ConfigManager.Get();
                logLevel = addOnConfig.LogLevel switch
                {
                    "info" => LogLevel.Information,
                    "debug" => LogLevel.Debug,
                    "trace" => LogLevel.Trace,
                    "error" => LogLevel.Error,
                    "warning" => LogLevel.Warning,
                    _ => LogLevel.Information
                };
                config.AddYamlAppConfig(
                    addOnConfig.ApplicationConfigFolderPath);
                
            })
            .ConfigureLogging((_, builder) => builder.SetMinimumLevel(logLevel));
    }
}