#region

using HiveMQtt.Client;
using HiveMQtt.MQTT5.Types;
using Microsoft.Extensions.Logging;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;

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
        var mqttClient = await _assuredMqttConnection.GetClientAsync();

        await PublishMessage(mqttClient, topic, payload, retain, qos);
    }

    private async Task PublishMessage(
        HiveMQClient mqttClient,
        string topic,
        string payload,
        bool retain,
        MqttQualityOfServiceLevel qos)
    {
        var message = new PublishMessageBuilder().WithTopic(topic)
            .WithPayload(payload)
            .WithRetain(retain)
            .WithQualityOfService((QualityOfService) qos)
            .Build();

        _logger.LogTrace("MQTT sending to {Topic}: {Message}", message.Topic, message.PayloadAsString);

        try
        {
            await mqttClient.PublishAsync(message).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to publish MQTT message to to {Topic}: {Message}", message.Topic, message.PayloadAsString);
            throw new MqttPublishException(e.Message, e);
        }
    }
}
