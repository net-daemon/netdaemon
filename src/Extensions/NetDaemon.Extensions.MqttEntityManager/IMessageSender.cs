namespace NetDaemon.Extensions.MqttEntityManager;

public interface IMessageSender
{
    Task SendMessageAsync(string topic, string payload);
}