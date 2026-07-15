namespace NetDaemon.Extensions.MqttEntityManager.Exceptions;

/// <summary>
/// MQTT connection failed
/// </summary>
public class MqttConnectionException : Exception
{
    /// <summary>
    /// MQTT connection failed
    /// </summary>
    /// <param name="msg">The exception message.</param>
    public MqttConnectionException(string msg) : base(msg)
    {}

    /// <summary>
    /// MQTT connection failed
    /// </summary>
    /// <param name="msg">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MqttConnectionException(string msg, Exception innerException) : base(msg, innerException)
    {}

    /// <summary>
    /// MQTT connection failed
    /// </summary>
    public MqttConnectionException()
    {
    }
}
