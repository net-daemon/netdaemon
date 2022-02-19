using MQTTnet.Protocol;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Interface to send messages to MQTT
/// </summary>
internal interface IMessageSender
{
    /// <summary>
    ///     Send a message for the given payload to the MQTT topic
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="payload"></param>
    /// <param name="retain"></param>
    /// <param name="qos"></param>
    /// <returns></returns>
    Task SendMessageAsync(string topic, string payload, bool retain, MqttQualityOfServiceLevel qos);
}