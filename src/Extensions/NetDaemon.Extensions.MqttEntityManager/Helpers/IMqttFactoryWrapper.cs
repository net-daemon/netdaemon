using MQTTnet;

namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

/// <summary>
/// Testable wrapper around IMqttFactory
/// </summary>
internal interface IMqttFactoryWrapper
{
    /// <summary>
    /// Return a managed MQTT client, either from the original factory or a pre-supplied one
    /// </summary>
    /// <returns></returns>
    IMqttClient CreateMqttClient();
}
