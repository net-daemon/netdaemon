using MQTTnet;

namespace NetDaemon.Extensions.MqttEntityManager.Helpers;

/// <summary>
/// Testable wrapper around IMqttFactory
/// </summary>
internal class MqttFactoryWrapper : IMqttFactoryWrapper
{
    private readonly IMqttFactory? _mqttFactory;
    private readonly IMqttClient? _client;

    /// <summary>
    /// Standard functionality - set the IMqttFactory that will return a client
    /// </summary>
    /// <param name="mqttFactory"></param>
    public MqttFactoryWrapper(IMqttFactory mqttFactory)
    {
        _mqttFactory = mqttFactory;
    }

    /// <summary>
    /// Testing functionality - specify a client that will be returned
    /// </summary>
    /// <param name="client"></param>
    public MqttFactoryWrapper(IMqttClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Return a managed MQTT client, either from the original factory or a pre-supplied one
    /// </summary>
    /// <returns></returns>
    public IMqttClient CreateMqttClient()
    {
        return _client ?? _mqttFactory?.CreateMqttClient()
            ?? throw new InvalidOperationException("No client or MqttFactory specified");
    }
}
