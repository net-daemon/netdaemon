namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Configuration model for MQTT
/// </summary>
public class MqttConfiguration
{
    /// <summary>
    /// Port to connect on, defaults to 1883
    /// </summary>
    public int Port { get; set; } = 1883;
    
    /// <summary>
    /// Discovery Prefix, defaults to "homeassistant"
    /// </summary>
    public string DiscoveryPrefix { get; set; } = "homeassistant";
    
    /// <summary>
    /// Host address of MQTT broker
    /// </summary>
    public string Host { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID to connect to MQTT broker
    /// </summary>
    public string? Password { get; set; }
    
    /// <summary>
    /// Password to connect to MQTT broker
    /// </summary>
    public string? UserId { get; set; }
}