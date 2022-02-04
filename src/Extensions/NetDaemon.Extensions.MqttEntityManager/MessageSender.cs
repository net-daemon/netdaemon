using System.Security.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;
using MQTTnet.Exceptions;

namespace NetDaemon.Extensions.MqttEntityManager;

internal class MessageSender : IMessageSender
{
    private readonly ILogger<MessageSender> _logger;
    private readonly IMqttFactory           _mqttFactory;
    private readonly MqttConfiguration      _mqttConfig;

    public MessageSender(ILogger<MessageSender> logger, IConfiguration configuration, IMqttFactory mqttFactory, IOptions<MqttConfiguration> mqttConfig)
    {
        _logger      = logger;
        _mqttFactory = mqttFactory;
        _mqttConfig  = mqttConfig.Value;

        if (string.IsNullOrEmpty(_mqttConfig.Host))
            throw new MqttConfigurationException("The Mqtt config was not found or there was an error loading it. Please add MqttConfiguration section to appsettings.json");

        _logger.LogDebug($"MQTT connection is {_mqttConfig.Host}:{_mqttConfig.Port}/{_mqttConfig.UserId}");
    }

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

        var connectResult = await mqttClient.ConnectAsync(options, CancellationToken.None).ConfigureAwait(false);
        if (connectResult.ResultCode != MqttClientConnectResultCode.Success)
            throw new AuthenticationException(connectResult.ReasonString);
    }

    private async Task PublishMessage(IApplicationMessagePublisher client, string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder().WithTopic(topic)
                                                         .WithPayload(payload)
                                                         .WithRetainFlag()
                                                         .Build();

        _logger.LogDebug($"Sending to {message.Topic}: {message.ConvertPayloadToString()}");

        var publishResult = await client.PublishAsync(message, CancellationToken.None).ConfigureAwait(false);
        if (publishResult.ReasonCode != MqttClientPublishReasonCode.Success)
            throw new InvalidOperationException(publishResult.ReasonString);
    }
}