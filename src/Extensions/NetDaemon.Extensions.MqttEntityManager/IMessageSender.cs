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
    /// <param name="topic">The MQTT topic.</param>
    /// <param name="payload">The message payload.</param>
    /// <param name="retain">Whether the message should be retained.</param>
    /// <param name="qos">The MQTT quality of service level.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendMessageAsync(string topic, string payload, bool retain, MqttQualityOfServiceLevel qos);
}
