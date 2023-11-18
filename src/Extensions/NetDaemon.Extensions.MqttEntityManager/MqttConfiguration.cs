namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Configuration model for MQTT
/// </summary>
public class MqttConfiguration
{
    /// <summary>
    /// Host address of MQTT broker
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Port to connect on, defaults to 1883
    /// </summary>
    public int Port { get; set; } = 1883;
    
    /// <summary>
    /// MQTT client ID
    /// </summary>
    public string? ClientId { get; set; }
    
    /// <summary>
    /// Discovery Prefix, defaults to "homeassistant"
    /// </summary>
    public string DiscoveryPrefix { get; set; } = "homeassistant";
    
    /// <summary>
    /// User name to connect to MQTT broker
    /// </summary>
    public string? UserName { get; set; }
    
    /// <summary>
    /// Password to connect to MQTT broker
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Use TLS to connect to MQTT broker, defaults to false
    /// </summary>
    public bool UseTls { get; set; }

    /// <summary>
    /// Whether or not to allow certificates which are untrusted by the
    /// operating system's certificate store, defaults to false
    /// </summary>
    public bool AllowUntrustedCertificates { get; set; }

    /// <summary>
    /// Whether or not to ignore issues verifying the broker's certificate
    /// chain, defaults to false
    /// </summary>
    public bool IgnoreCertificateChainErrors { get; set; }

    /// <summary>
    /// Whether or not to ignore certificate revocation errors, defaults
    /// to false
    /// </summary>
    public bool IgnoreCertificateRevocationErrors { get; set; }
}
