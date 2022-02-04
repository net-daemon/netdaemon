namespace NetDaemon.Extensions.MqttEntityManager;

internal interface IMessageSender
{
    Task SendMessageAsync(string topic, string payload);
}