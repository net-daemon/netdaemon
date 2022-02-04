#region

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Exceptions;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;

#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Manage connections and message publishing to MQTT
/// </summary>
internal class MessageSender : IMessageSender
{
    private readonly ILogger<MessageSender> _logger;
    private readonly MqttConfiguration      _mqttConfig;
    private readonly IMqttFactory           _mqttFactory;

    /// <summary>
    /// Manage connections and message publishing to MQTT
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mqttFactory"></param>
    /// <param name="mqttConfig"></param>
    /// <exception cref="MqttConfigurationException"></exception>
    public MessageSender(ILogger<MessageSender> logger, IMqttFactory mqttFactory, IOptions<MqttConfiguration> mqttConfig)
    {
        _logger      = logger;
        _mqttFactory = mqttFactory;
        _mqttConfig  = mqttConfig.Value;

        if (string.IsNullOrEmpty(_mqttConfig.Host))
            throw new MqttConfigurationException("The Mqtt config was not found or there was an error loading it. Please add MqttConfiguration section to appsettings.json");

        _logger.LogDebug("MQTT connection is {host}:{port}/{userId}", _mqttConfig.Host, _mqttConfig.Port, _mqttConfig.UserId);
    }

    /// <summary>
    /// Connect to MQTT and publish a message to the given topic
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="payload">Json structure of payload</param>
    public async Task SendMessageAsync(string topic, string payload)
    {
        using var mqttClient = _mqttFactory.CreateMqttClient();
        await ConnectAsync(mqttClient);
        await PublishMessage(mqttClient, topic, payload);
    }

    private async Task ConnectAsync(IMqttClient mqttClient)
    {
        var options = new MqttClientOptionsBuilder()
                      .WithTcpServer(_mqttConfig.Host, _mqttConfig.Port)
                      .WithCredentials(_mqttConfig.UserId, _mqttConfig.Password)
                      .Build();

        try
        {
            var connectResult =
                await mqttClient.ConnectAsync(options, CancellationToken.None).ConfigureAwait(false);
            if (connectResult.ResultCode != MqttClientConnectResultCode.Success)
                throw new MqttConnectionException(connectResult.ReasonString);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new MqttConnectionException(e.Message, e);
        }
    }

    private async Task PublishMessage(IApplicationMessagePublisher client, string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder().WithTopic(topic)
                                                         .WithPayload(payload)
                                                         .WithRetainFlag()
                                                         .Build();

        _logger.LogDebug("Sending to {topic}: {message}", message.Topic, message.ConvertPayloadToString());

        try
        {
            var publishResult = await client.PublishAsync(message, CancellationToken.None).ConfigureAwait(false);
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