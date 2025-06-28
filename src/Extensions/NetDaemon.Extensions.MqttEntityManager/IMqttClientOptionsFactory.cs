using MQTTnet;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Represents a factory for creating MQTT client options.
/// </summary>
public interface IMqttClientOptionsFactory
{
    /// <summary>
    /// Creates the client options for MQTT connection from the supplied configuration.
    /// /// </summary>
    /// <param name="mqttConfig">The MQTT configuration.</param>
    /// <returns>The managed MQTT client options.</returns>
    MqttClientOptions CreateClientOptions(MqttConfiguration mqttConfig);
}
