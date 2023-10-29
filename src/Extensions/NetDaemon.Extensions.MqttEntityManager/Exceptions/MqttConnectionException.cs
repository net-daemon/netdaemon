namespace NetDaemon.Extensions.MqttEntityManager.Exceptions;

/// <summary>
/// MQTT connection failed
/// </summary>
public class MqttConnectionException : Exception
{
    /// <summary>
    /// MQTT connection failed
    /// </summary>
    /// <param name="msg"></param>
    public MqttConnectionException(string msg) : base(msg)
    {}

    /// <summary>
    /// MQTT connection failed
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="innerException"></param>
    public MqttConnectionException(string msg, Exception innerException) : base(msg, innerException)
    {}

    /// <summary>
    /// MQTT connection failed
    /// </summary>
    public MqttConnectionException()
    {
    }
}
