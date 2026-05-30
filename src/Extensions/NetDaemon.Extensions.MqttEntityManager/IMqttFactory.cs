using MQTTnet;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Factory abstraction for MQTT clients.
/// </summary>
internal interface IMqttFactory
{
    /// <summary>
    /// Create an MQTT client.
    /// </summary>
    /// <returns>The MQTT client.</returns>
    IMqttClient CreateMqttClient();
}
