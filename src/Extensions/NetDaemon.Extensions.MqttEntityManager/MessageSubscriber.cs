#region

using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Packets;

#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Handle subscriptions to topics within MQTT and provide access for multiple subscribers to
///     receive updates
/// </summary>
internal class MessageSubscriber : IMessageSubscriber, IDisposable
{
    private readonly SemaphoreSlim _subscriptionSetupLock = new SemaphoreSlim(1);
    private volatile bool _isDisposed;
    private bool _subscriptionIsSetup;
    private readonly IAssuredMqttConnection _assuredMqttConnection;
    private readonly ILogger<MessageSubscriber> _logger;
    private readonly ConcurrentDictionary<string, Lazy<Subject<string>>> _subscribers = new();

    /// <summary>
    ///     Managed subscriptions to topics within MQTT
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="assuredMqttConnection"></param>
    public MessageSubscriber(ILogger<MessageSubscriber> logger, IAssuredMqttConnection assuredMqttConnection)
    {
        _logger = logger;
        _assuredMqttConnection = assuredMqttConnection;
    }

    /// <summary>
    ///     Subscribe to the given topic
    /// </summary>
    /// <param name="topic"></param>
    public async Task<IObservable<string>> SubscribeTopicAsync(string topic)
    {
        try
        {
            await EnsureSubscriptionAsync();

            var topicFilter = new MqttTopicFilterBuilder().WithTopic(topic).Build();

            await _assuredMqttConnection.SubscribeAsync(topicFilter);
            return _subscribers.GetOrAdd(topic, new Lazy<Subject<string>>()).Value;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to subscribe to topic");
            throw;
        }
    }

    /// <summary>
    /// If we are not already subscribed to receive messages, set up the handler
    /// </summary>
    private async Task EnsureSubscriptionAsync()
    {
        await _subscriptionSetupLock.WaitAsync();
        try
        {
            if (!_subscriptionIsSetup)
            {
                _logger.LogInformation("Configuring message subscription");
                _assuredMqttConnection.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
                _subscriptionIsSetup = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set up or process message subscription");
            throw;
        }
        finally
        {
            _subscriptionSetupLock.Release();
        }
    }

    /// <summary>
    /// Message received from MQTT, so find the subscription (if any) and notify them
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs msg)
    {
        try
        {
            var payload = msg.ApplicationMessage.ConvertPayloadToString();
            var topic = msg.ApplicationMessage.Topic;
            _logger.LogTrace("Subscription received {Payload} from {Topic}", payload, topic);

            if (!_subscribers.ContainsKey(topic))
                _logger.LogTrace("No subscription for topic={Topic}", topic);
            else
            {
                _subscribers[topic].Value.OnNext(payload);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to notify subscribers");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        foreach (var observer in _subscribers)
        {
            _logger.LogTrace("Disposing {Topic} subscription", observer.Key);
            observer.Value.Value.OnCompleted();
            observer.Value.Value.Dispose();
        }

        _subscriptionSetupLock.Dispose();
    }
}
