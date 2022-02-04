namespace NetDaemon.Extensions.MqttEntityManager.Exceptions;

public class MqttPublishException : Exception
{
    public MqttPublishException(string msg) : base(msg)
    {}
    
    public MqttPublishException(string msg, Exception innerException) : base(msg, innerException)
    {}
}