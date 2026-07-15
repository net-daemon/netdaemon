namespace NetDaemon.Extensions.MqttEntityManager;

internal interface IMessageSubscriber
{
    /// <summary>
    ///     Receive a message from the given topic
    /// </summary>
    /// <param name="topic">The MQTT topic.</param>
    /// <returns>An observable that receives payloads for the topic.</returns>
    Task<IObservable<string>> SubscribeTopicAsync(string topic);
}
