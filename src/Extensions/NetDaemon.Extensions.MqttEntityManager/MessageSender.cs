#region

using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
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

    private async Task PublishMessage(IManagedMqttClient mqttClient, string topic, string payload, bool retain,
        MqttQualityOfServiceLevel qos)
    {
        var message = new MqttApplicationMessageBuilder().WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(retain)
            .WithQualityOfServiceLevel(qos)
            .Build();

        _logger.LogTrace("MQTT sending to {Topic}: {Message}", message.Topic, message.ConvertPayloadToString());

        try
        {
            await mqttClient.EnqueueAsync(message).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw new MqttPublishException(e.Message, e);
        }
    }
}