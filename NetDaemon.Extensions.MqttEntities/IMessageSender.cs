namespace NetDaemon.Extensions.MqttEntities;

internal interface IMessageSender
{
    Task SendMessageAsync(string topic, string payload);
}