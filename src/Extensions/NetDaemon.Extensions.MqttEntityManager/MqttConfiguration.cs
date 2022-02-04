namespace NetDaemon.Extensions.MqttEntityManager;

public class MqttConfiguration
{
    public int Port { get; set; } = 1883;
    public string DiscoveryPrefix { get; set; } = "homeassistant";
    public string Host { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? UserId { get; set; }
}