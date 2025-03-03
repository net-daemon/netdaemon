using HiveMQtt.Client;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Defines a factory interface for creating MQTT client instances.
/// </summary>
/// <remarks>
/// This interface abstracts the creation of MQTT clients, allowing for flexibility
/// in providing different implementations or configurations for MQTT connections.
/// It is primarily used to encapsulate the instantiation logic for MQTT clients
/// within the application.
/// </remarks>
internal interface IMqttClientFactory
{
    IHiveMqClientWrapper GetClient();
}
