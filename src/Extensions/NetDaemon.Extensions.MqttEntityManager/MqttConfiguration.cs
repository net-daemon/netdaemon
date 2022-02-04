namespace NetDaemon.Extensions.MqttEntityManager;

internal class MqttConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1883;
    public string? UserId { get; set; }
    public string? Password { get; set; }
}