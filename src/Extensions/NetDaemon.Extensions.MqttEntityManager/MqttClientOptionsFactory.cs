using MQTTnet.Extensions.ManagedClient;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <inheritdoc />
public class MqttClientOptionsFactory : IMqttClientOptionsFactory
{
    /// <inheritdoc />
    public ManagedMqttClientOptions CreateClientOptions(MqttConfiguration mqttConfig)
    {
        ArgumentNullException.ThrowIfNull(mqttConfig);

        if (string.IsNullOrEmpty(mqttConfig.Host))
        {
            throw new ArgumentException("Explicit MQTT host configuration was not provided and no suitable broker addon was discovered", nameof(mqttConfig));
        }

        var clientOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(clientOptionsBuilder =>
            {
                clientOptionsBuilder.WithTcpServer(mqttConfig.Host, mqttConfig.Port);

                if (!string.IsNullOrEmpty(mqttConfig.UserName) && !string.IsNullOrEmpty(mqttConfig.Password))
                {
                    clientOptionsBuilder.WithCredentials(mqttConfig.UserName, mqttConfig.Password);
                }

                if (!string.IsNullOrEmpty(mqttConfig.ClientId))
                {
                    clientOptionsBuilder.WithClientId(mqttConfig.ClientId);
                }

                if (mqttConfig.UseTls)
                {
                    clientOptionsBuilder.WithTlsOptions(tlsOptionsBuilder =>
                    {
                        tlsOptionsBuilder
                            .UseTls()
                            .WithAllowUntrustedCertificates(mqttConfig.AllowUntrustedCertificates)
                            .WithIgnoreCertificateChainErrors(mqttConfig.IgnoreCertificateChainErrors)
                            .WithIgnoreCertificateRevocationErrors(mqttConfig.IgnoreCertificateRevocationErrors);
                    });
                }
            })
            .Build();

        return clientOptions;
    }
}
