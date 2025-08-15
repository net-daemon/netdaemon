using MQTTnet;

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

        var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttConfig.Host, mqttConfig.Port);

        if (!string.IsNullOrEmpty(mqttConfig.UserName) && !string.IsNullOrEmpty(mqttConfig.Password))
        {
            clientOptions = clientOptions.WithCredentials(mqttConfig.UserName, mqttConfig.Password);
        }

        if (mqttConfig.UseTls)
        {
            clientOptions = clientOptions.WithTlsOptions(tlsOptionsBuilder =>
            {
                tlsOptionsBuilder
                    .UseTls()
                    .WithAllowUntrustedCertificates(mqttConfig.AllowUntrustedCertificates);
            });
        }

        return clientOptions.Build();

    }
}
