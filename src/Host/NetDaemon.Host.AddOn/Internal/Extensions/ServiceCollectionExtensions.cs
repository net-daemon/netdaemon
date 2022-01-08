using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.Client.Common.Settings;
using NetDaemon.Host.AddOn.Internal.Config;

namespace NetDaemon.Runtime.Internal.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNetDaemonAddOnConfiguration(this IServiceCollection services)
    {
        var config = ConfigManager.Get();
        var token = Environment.GetEnvironmentVariable("HASSIO_TOKEN") ??
                    throw new InvalidOperationException(
                        "Expected HASSIO_TOKEN to be present in environment. Is this running as addon?");

        var appFolderPath = Path.Combine("/config/netdaemon3", config.ApplicationConfigFolderPath);
        services.Configure<HomeAssistantSettings>(n =>
        {
            n.Host = "supervisor";

            n.Port = 80;
            n.Token = token;
            n.Ssl = false;
            n.WebsocketPath = "core/websocket";
        });

        services.Configure<AppConfigurationLocationSetting>(s => s.ApplicationConfigurationFolder = appFolderPath);

        return services;
    }
}