using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace NetDaemon.Extensions.MqttEntities;

public static class DependencyInjectionSetup
{
    /// <summary>
    ///     Adds scheduling capabilities through dependency injection
    /// </summary>
    /// <param name="services">Provided service collection</param>
    public static IServiceCollection AddMqttExtensions(this IServiceCollection services)
    {
        services.AddSingleton<IMqttFactory, MqttFactory>();
        services.AddSingleton<IEntityUpdater, EntityUpdater>();
        services.AddSingleton<IMessageSender, MessageSender>();
        
        return services;
    }
}