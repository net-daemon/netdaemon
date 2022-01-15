using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
            .ConfigureServices((context, services) =>
            {
                services
                    .AddAppsFromSource()
                    .AddNetDaemonScheduler()
                    .AddNetDameonStateManager();
            });
    }

    private static IHostBuilder UseNetDaemonAddOnSettings(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureServices((context, services) => { services.AddNetDaemonAddOnConfiguration(); })
            .ConfigureAppConfiguration((ctx, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json");

                var addOnConfig = ConfigManager.Get();
                config.AddYamlAppConfig(
                    addOnConfig.ApplicationConfigFolderPath);
            });
    }
}