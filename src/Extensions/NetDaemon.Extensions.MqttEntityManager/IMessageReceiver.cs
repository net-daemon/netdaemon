namespace NetDaemon.Extensions.MqttEntityManager;

public interface IMessageReceiver
{
    IObservable<string> Messages { get; }

    /// <summary>
    ///     Receive a message from the given topic
    /// </summary>
    /// <param name="topic"></param>
    Task ReceiveMessageAsync(string topic);
}