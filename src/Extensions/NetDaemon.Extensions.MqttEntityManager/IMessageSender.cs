namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Interface to send messages to MQTT
/// </summary>
public interface IMessageSender
{
    /// <summary>
    ///     Send a message for the given payload to the MQTT topic
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="payload"></param>
    /// <param name="retain"></param>
    /// <returns></returns>
    Task SendMessageAsync(string topic, string payload, bool retain = false);
}