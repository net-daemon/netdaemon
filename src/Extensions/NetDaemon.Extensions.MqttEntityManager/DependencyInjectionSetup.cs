#region

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// DI setup for Mqtt Entity Manager
/// </summary>
public static class DependencyInjectionSetup
{
    /// <summary>
    /// Add support for managing entities via MQTT
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <returns></returns>
    public static IHostBuilder UseNetDaemonMqttEntityManagement(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IAssuredMqttConnection, AssuredHiveMqttConnection>();
            services.AddSingleton<IMessageSender, MessageSender>();
            services.AddSingleton<IMessageSubscriber, MessageSubscriber>();
            services.AddSingleton<IMqttClientFactory, HiveMqttClientFactory>();
            services.AddSingleton<IMqttEntityManager, MqttEntityManager>();
            services.Configure<MqttConfiguration>(context.Configuration.GetSection("Mqtt"));
        });
    }
}
