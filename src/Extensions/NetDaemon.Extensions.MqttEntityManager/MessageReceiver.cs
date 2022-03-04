#region

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;

#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Manage connections and message publishing to MQTT
/// </summary>
internal class MessageReceiver : IMessageReceiver
{
    private readonly IAssuredMqttConnection _assuredMqttConnection;
    private readonly ILogger<MessageSender> _logger;

    /// <summary>
    ///     Manage connections and message publishing to MQTT
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="assuredMqttConnection"></param>
    public MessageReceiver(ILogger<MessageSender> logger, IAssuredMqttConnection assuredMqttConnection)
    {
        _logger                = logger;
        _assuredMqttConnection = assuredMqttConnection;
    }

    public IObservable<string> Messages { get; private set; }

    /// <summary>
    ///     Publish a message to the given topic
    /// </summary>
    /// <param name="topic"></param>
    public async Task ReceiveMessageAsync(string topic)
    {
        var mqttClient = await _assuredMqttConnection.GetClientAsync();

        await ReceiveMessage(mqttClient, topic);
    }

    private async Task ReceiveMessage(IManagedMqttClient mqttClient, string topic)
    {
        try
        {
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            Messages = Observable.Create<string>(observer =>
            {
                mqttClient.UseApplicationMessageReceivedHandler(
                    msg =>
                    {
                        var payload = Encoding.UTF8.GetString(msg.ApplicationMessage.Payload);
                        observer.OnNext(payload);
                        _logger.LogInformation("Received: '{payload}' from '{topic}'",
                            Encoding.UTF8.GetString(msg.ApplicationMessage.Payload),
                            msg.ApplicationMessage.Topic);
                    });
                return Disposable.Empty;
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }
}