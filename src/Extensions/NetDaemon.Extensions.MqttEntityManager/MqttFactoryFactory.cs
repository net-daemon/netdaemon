using MQTTnet;
using MQTTnet.Extensions.ManagedClient;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// MqttNet removed the IMqttFactory interface at v4 which breaks our DI
/// So this is a factory for the MqttFactory to satisfy our use case
/// </summary>
internal class MqttFactoryFactory : IMqttFactory
{
    /// <summary>
    /// Create a Managed Mqtt Client
    /// </summary>
    /// <returns></returns>
    public IManagedMqttClient CreateManagedMqttClient()
    {
        return new MqttFactory().CreateManagedMqttClient();
    }
}