using MQTTnet;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Factory wrapper for MQTTnet's client factory.
/// </summary>
internal class MqttFactoryFactory : IMqttFactory
{
    /// <inheritdoc />
    public IMqttClient CreateMqttClient()
    {
        return new MqttClientFactory().CreateMqttClient();
    }
}
