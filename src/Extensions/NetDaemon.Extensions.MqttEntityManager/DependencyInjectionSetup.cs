using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;

namespace NetDaemon.Extensions.MqttEntityManager;

public static class DependencyInjectionSetup
{
    public static IHostBuilder UseNetDaemonMqttEntityManagement(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddSingleton<IMqttFactory, MqttFactory>();
            services.AddSingleton<IMqttEntityManager, MqttEntityManager>();
            services.AddTransient<IMessageSender, MessageSender>();
            services.Configure<MqttConfiguration>(context.Configuration.GetSection(nameof(MqttConfiguration)));
        });
    }
}