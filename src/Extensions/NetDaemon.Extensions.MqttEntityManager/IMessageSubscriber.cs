namespace NetDaemon.Extensions.MqttEntityManager;

internal interface IMessageSubscriber
{
    /// <summary>
    ///     Receive a message from the given topic
    /// </summary>
    /// <param name="topic"></param>
    Task<IObservable<string>> SubscribeTopicAsync(string topic);
}