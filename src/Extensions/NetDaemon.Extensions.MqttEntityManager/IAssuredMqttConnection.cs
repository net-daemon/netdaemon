using MQTTnet.Extensions.ManagedClient;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection
/// </summary>
internal interface IAssuredMqttConnection
{
    /// <summary>
    /// Ensures that an MQTT client is available, retrying if necessary, and throws if connection is impossible
    /// </summary>
    IManagedMqttClient GetClientOrThrow();
}