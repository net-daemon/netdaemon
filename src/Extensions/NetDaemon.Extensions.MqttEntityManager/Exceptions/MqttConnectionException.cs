namespace NetDaemon.Extensions.MqttEntityManager.Exceptions;

public class MqttConnectionException : Exception
{
    public MqttConnectionException(string msg) : base(msg)
    {}
    
    public MqttConnectionException(string msg, Exception innerException) : base(msg, innerException)
    {}
}