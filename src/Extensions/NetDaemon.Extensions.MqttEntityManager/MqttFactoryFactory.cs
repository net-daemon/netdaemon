using MQTTnet;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Factory wrapper for MQTTnet's client factory.
/// </summary>
internal class MqttFactoryFactory : IMqttFactory
{
    /// <summary>
    /// Create an MQTT client.
    /// </summary>
    /// <returns></returns>
    public IMqttClient CreateMqttClient()
    {
        return new MqttClientFactory().CreateMqttClient();
    }
}
