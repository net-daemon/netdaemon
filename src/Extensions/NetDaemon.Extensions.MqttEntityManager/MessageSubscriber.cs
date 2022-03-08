﻿#region
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using NetDaemon.Extensions.MqttEntityManager.Helpers;

#endregion

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Handle subscriptions to topics within MQTT and provide access for multiple subscribers to
///     receive updates
/// </summary>
internal class MessageSubscriber : IMessageSubscriber, IDisposable
{
    private readonly SemaphoreSlim _subscriptionSetupLock = new SemaphoreSlim(1);
    private readonly SemaphoreSlim _subscriptionListLock = new SemaphoreSlim(1);
    private bool _isDisposed;
    private bool _subscriptionIsSetup;
    private readonly IAssuredMqttConnection _assuredMqttConnection;
    private readonly ILogger<MessageSubscriber> _logger;
    private readonly List<Subject<string>> _subscribedTopics = new();
    private readonly Dictionary<string, Subject<string>> _subscribers = new();
    
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
            var mqttClient = await _assuredMqttConnection.GetClientAsync();
            await EnsureSubscriptionAsync(mqttClient);

            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
            return await AddSubscription(topic);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to subscribe to topic");
            throw;
        }
    }

    private async Task<IObservable<string>> AddSubscription(string topic)
    {
        await _subscriptionListLock.WaitAsync();
        try
        {
            if (!_subscribers.ContainsKey(topic))
            {
                var subject = new Subject<string>();
                _subscribedTopics.Add(subject);
                _subscribers.Add(topic, subject);
            }
            return _subscribers[topic];
        }
        finally
        {
            _subscriptionListLock.Release();
        }
    }

    /// <summary>
    /// If we are not already subscribed to receive messages, set up the handler
    /// </summary>
    /// <param name="mqttClient"></param>
    private async Task EnsureSubscriptionAsync(IManagedMqttClient mqttClient)
    {
        await _subscriptionSetupLock.WaitAsync();
        try
        {
            if (!_subscriptionIsSetup)
            {
                _logger.LogInformation("Configuring message subscription");
                mqttClient.UseApplicationMessageReceivedHandler(OnMessageReceived);
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
    private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs msg)
    {
        try
        {
            var payload = ByteArrayHelper.SafeToString(msg.ApplicationMessage.Payload);
            var topic = msg.ApplicationMessage.Topic;
            _logger.LogDebug("Subscription received {Payload} from {Topic}", payload, topic);

            if (!_subscribers.ContainsKey(topic))
                _logger.LogDebug("No subscription for topic={Topic}", topic);
            else
            {
                _subscribers[topic].OnNext(payload);
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
        if (!_isDisposed)
        {
            _isDisposed = true;
            foreach (var observer in _subscribers)
            {
                _logger.LogDebug("Disposing {Topic} subscription", observer.Key);
                observer.Value.OnCompleted();
                observer.Value.Dispose();
            }

            foreach (var subscribedTopic in _subscribedTopics)
            {
                subscribedTopic.Dispose();
            }
            
            _subscriptionSetupLock.Dispose();
            _subscriptionListLock.Dispose();
        }
    }
}