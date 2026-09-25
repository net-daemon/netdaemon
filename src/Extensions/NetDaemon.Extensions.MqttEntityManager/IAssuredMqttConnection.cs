using MQTTnet;
using MQTTnet.Packets;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection.
/// </summary>
internal interface IAssuredMqttConnection
{
    /// <summary>
    /// Raised when an MQTT application message is received.
    /// </summary>
    event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceivedAsync;

    /// <summary>
    /// Queue a message to publish to MQTT.
    /// </summary>
    Task PublishAsync(MqttApplicationMessage message);

    /// <summary>
    /// Subscribe to a topic, retaining the subscription across reconnects.
    /// </summary>
    Task SubscribeAsync(MqttTopicFilter topicFilter);
}
