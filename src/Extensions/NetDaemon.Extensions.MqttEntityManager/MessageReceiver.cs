#region
using System.Reactive.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;
#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Manage connections and message publishing to MQTT
/// </summary>
internal class MessageReceiver : IMessageReceiver, IDisposable
{
    private readonly SemaphoreSlim _subscriptionSetupLock = new SemaphoreSlim(1);
    private readonly SemaphoreSlim _subscriptionListLock = new SemaphoreSlim(1);
    private bool _isDisposed;
    private bool _subscribtionIsSetup;
    private readonly IAssuredMqttConnection _assuredMqttConnection;
    private readonly ILogger<MessageReceiver> _logger;
    private Dictionary<string, TopicObservers> _subscriptions = new Dictionary<string, TopicObservers>();

    public class TopicObservers
    {
        public IObservable<string> Observable { get; set; }
        public IObserver<string> Observer { get; set; }
    }
    
    /// <summary>
    ///     Manage connections and message subscription by MQTT
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="assuredMqttConnection"></param>
    public MessageReceiver(ILogger<MessageReceiver> logger, IAssuredMqttConnection assuredMqttConnection)
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
        var mqttClient = await _assuredMqttConnection.GetClientAsync();
        await EnsureSubscriptionAsync(mqttClient);

        await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
        return await AddSubscription(topic);
    }


    private async Task<IObservable<string>> AddSubscription(string topic)
    {
        if (_subscriptions.ContainsKey(topic))
            throw new MqttDuplicateTopicSubscriptionException($"The topic '{topic}' is already subscribed");

        await _subscriptionListLock.WaitAsync();

        var subscription = new TopicObservers();
        subscription.Observable = Observable.Create<string>(
            (IObserver<string> observer) =>
            {
                subscription.Observer = observer;
                return () => _logger.LogDebug("Topic {Topic} is unsubscribing", topic);
            });

        _subscriptions.Add(topic, subscription);
        _subscriptionListLock.Release();

        return subscription.Observable;
    }

    private async Task EnsureSubscriptionAsync(IManagedMqttClient mqttClient)
    {
        try
        {
            await _subscriptionSetupLock.WaitAsync();
            if (!_subscribtionIsSetup)
            {
                _logger.LogInformation("Configuring message subscription");
                mqttClient.UseApplicationMessageReceivedHandler(msg =>
                    {
                        var payload = Encoding.UTF8.GetString(msg.ApplicationMessage.Payload);
                        var topic = msg.ApplicationMessage.Topic;
                        _logger.LogDebug("Subscription received {Payload} from {Topic}", payload, topic);

                        if (!_subscriptions.ContainsKey(topic))
                            _logger.LogDebug("No subscription for topic={Topic}", topic);
                        else
                            (_subscriptions[topic]).Observer.OnNext(payload);
                    }
                );
                _subscribtionIsSetup = true;
            }

            _subscriptionSetupLock.Release();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set up or process message subscription");
            throw;
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            foreach (var observer in _subscriptions.Values.Select(s => s.Observer))
            {
                observer.OnCompleted();
            }
            _subscriptionSetupLock.Dispose();
            _subscriptionSetupLock.Dispose();
        }
    }
}