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

    /// <summary>
    ///     Publish a message to the given topic
    /// </summary>
    /// <param name="topic"></param>
    public async Task<IObservable<string>> ReceiveMessageAsync(string topic)
    {
        var mqttClient = await _assuredMqttConnection.GetClientAsync();
        await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
        return ReceiveMessage(mqttClient, topic);
    }

    private IObservable<string> ReceiveMessage(IManagedMqttClient mqttClient, string topic)
    {
        try
        {
            _logger.LogInformation("Subscribed to {Topic}", topic);
            return Observable.Create<string>(observer =>
            {
                mqttClient.UseApplicationMessageReceivedHandler(
                    msg =>
                    {
                        var payload = Encoding.UTF8.GetString(msg.ApplicationMessage.Payload);
                        _logger.LogDebug("Received: '{payload}' from '{topic}'",
                            Encoding.UTF8.GetString(msg.ApplicationMessage.Payload),
                            msg.ApplicationMessage.Topic);
                        observer.OnNext(payload);
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