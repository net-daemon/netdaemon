namespace NetDaemon.Extensions.MqttEntityManager;

internal interface IMessageReceiver
{
    /// <summary>
    ///     Receive a message from the given topic
    /// </summary>
    /// <param name="topic"></param>
    Task<IObservable<string>> ReceiveMessageAsync(string topic);
}