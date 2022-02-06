#region

using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Publishing;
using MQTTnet.Extensions.ManagedClient;
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
    public async Task SendMessageAsync(string topic, string payload, bool retain = false)
    {
        var mqttClient = _assuredMqttConnection.GetClientOrThrow();
        
        await PublishMessage(mqttClient, topic, payload, retain);
    }

    private async Task PublishMessage(IManagedMqttClient mqttClient, string topic, string payload, bool retain)
    {
        var message = new MqttApplicationMessageBuilder().WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(retain)
            .Build();

        _logger.LogDebug("MQTT sending to {Topic}: {Message}", message.Topic, message.ConvertPayloadToString());

        try
        {
            var publishResult = await mqttClient.PublishAsync(message, CancellationToken.None).ConfigureAwait(false);
            if (publishResult.ReasonCode != MqttClientPublishReasonCode.Success)
                throw new MqttPublishException(publishResult.ReasonString);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw new MqttPublishException(e.Message, e);
        }
    }
}