namespace NetDaemon.Extensions.MqttEntityManager.Exceptions;

/// <summary>
/// Failed to publish a message to MQTT
/// </summary>
public class MqttPublishException : Exception
{
    /// <summary>
    /// Failed to publish a message to MQTT
    /// </summary>
    /// <param name="msg"></param>
    public MqttPublishException(string msg) : base(msg)
    {}
    
    /// <summary>
    /// Failed to publish a message to MQTT
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="innerException"></param>
    public MqttPublishException(string msg, Exception innerException) : base(msg, innerException)
    {}
}