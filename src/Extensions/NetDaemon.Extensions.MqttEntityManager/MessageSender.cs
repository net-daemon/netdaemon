#region

using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;
using System.Text;

#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Manage connections and message publishing to MQTT
/// </summary>
internal class MessageSender : IMessageSender
{
    private readonly ILogger<MessageSender> _logger;
    private readonly IAssuredMqttConnection _assuredMqttConnection;

    /// <summary>
    ///     Manage connections and message publishing to MQTT
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="assuredMqttConnection"></param>
    public MessageSender(ILogger<MessageSender> logger, IAssuredMqttConnection assuredMqttConnection)
    {
        _logger = logger;
        _assuredMqttConnection = assuredMqttConnection;
    }

    /// <summary>
    ///     Publish a message to the given topic
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="payload">Json structure of payload</param>
    /// <param name="retain"></param>
    /// <param name="qos"></param>
    public async Task SendMessageAsync(string topic, string payload, bool retain, MqttQualityOfServiceLevel qos)
    {
        await PublishMessage(topic, payload, retain, qos);
    }

    private async Task PublishMessage(string topic, string payload, bool retain, MqttQualityOfServiceLevel qos)
    {
        var message = new MqttApplicationMessageBuilder().WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(payload))
            .WithRetainFlag(retain)
            .WithQualityOfServiceLevel(qos)
            .Build();

        _logger.LogTrace("MQTT sending to {Topic}: {Message}", message.Topic, message.ConvertPayloadToString());

        try
        {
            await _assuredMqttConnection.PublishAsync(message).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish MQTT message to to {Topic}: {Message}", message.Topic, message.ConvertPayloadToString());
            throw new MqttPublishException(e.Message, e);
        }
    }
}
