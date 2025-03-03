using HiveMQtt.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Factory class for creating instances of HiveMQ MQTT clients.
/// </summary>
/// <remarks>
/// This class is responsible for creating and configuring HiveMQ MQTT clients based on the settings
/// provided via the injected MqttConfiguration instance. It also logs the connection details
/// for debugging purposes.
/// </remarks>
internal class HiveMqttClientFactory(
    ILogger<HiveMqttClientFactory> logger,
    IOptions<MqttConfiguration> mqttConfig
    ) : IMqttClientFactory
{
    /// <summary>
    /// Creates and returns a new instance of an IHiveMQClient configured using the
    /// current MQTT configuration settings.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IHiveMQClient"/> configured with the host, port,
    /// and optional username and password specified in the MQTT configuration.
    /// </returns>
    public IHiveMqClientWrapper GetClient()
    {
        var config = mqttConfig.Value;

        logger.LogDebug("MQTTClient creating client for {Host}:{Port}", config.Host, config.Port);

        var options = new HiveMQClientOptionsBuilder()
            .WithBroker(config.Host)
            .WithPort(config.Port)
            .WithAutomaticReconnect(true);

        if (!string.IsNullOrEmpty(config.UserName))
            options = options.WithUserName(config.UserName);
        if (!string.IsNullOrEmpty(config.Password))
            options = options.WithPassword(config.Password);

        return new HiveMqClientWrapper(new HiveMQClient(options.Build()));
    }
}
