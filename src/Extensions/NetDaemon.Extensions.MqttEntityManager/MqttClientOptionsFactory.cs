using MQTTnet;
using MQTTnet.Formatter;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <inheritdoc />
public class MqttClientOptionsFactory : IMqttClientOptionsFactory
{
    /// <inheritdoc />
    public MqttClientOptions CreateClientOptions(MqttConfiguration mqttConfig)
    {
        ArgumentNullException.ThrowIfNull(mqttConfig);

        if (string.IsNullOrEmpty(mqttConfig.Host))
        {
            throw new ArgumentException("Explicit MQTT host configuration was not provided and no suitable broker addon was discovered", nameof(mqttConfig));
        }

        var clientOptionsBuilder = new MqttClientOptionsBuilder()
            .WithProtocolVersion(MqttProtocolVersion.V311)
            .WithTcpServer(mqttConfig.Host, mqttConfig.Port);

        if (mqttConfig.UseTls)
        {
            clientOptionsBuilder.WithTlsOptions(tlsOptionsBuilder =>
            {
                tlsOptionsBuilder
                    .UseTls()
                    .WithAllowUntrustedCertificates(mqttConfig.AllowUntrustedCertificates);
            });
        }

        if (!string.IsNullOrEmpty(mqttConfig.UserName) && !string.IsNullOrEmpty(mqttConfig.Password))
        {
            clientOptionsBuilder.WithCredentials(mqttConfig.UserName, mqttConfig.Password);
        }

        return clientOptionsBuilder.Build();
    }
}
