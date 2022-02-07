using MQTTnet.Extensions.ManagedClient;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection
/// </summary>
internal interface IAssuredMqttConnection
{
    /// <summary>
    /// Ensures that the MQTT client is available
    /// </summary>
    Task<IManagedMqttClient> GetClientAsync();
}