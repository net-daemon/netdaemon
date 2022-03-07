namespace NetDaemon.Extensions.MqttEntityManager.Exceptions;

public class MqttDuplicateTopicSubscriptionException : Exception
{
    public MqttDuplicateTopicSubscriptionException(string msg)
        : base(msg)
    {
    }
}