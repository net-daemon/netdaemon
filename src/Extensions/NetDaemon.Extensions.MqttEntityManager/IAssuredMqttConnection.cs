using MQTTnet;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection
/// </summary>
public interface IAssuredMqttConnection
{
    /// <summary>
    /// Ensures that the MQTT client is available
    /// </summary>
    Task<IMqttClient> GetClientAsync();
}
