namespace NetDaemon.Extensions.MqttEntities;

public interface IMessageSender
{
    Task SendMessageAsync(string topic, string payload);
}