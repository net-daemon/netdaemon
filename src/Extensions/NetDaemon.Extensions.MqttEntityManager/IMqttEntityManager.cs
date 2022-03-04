using MQTTnet.Protocol;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Interface for managing entities via MQTT
/// </summary>
public interface IMqttEntityManager
{
    /// <summary>
    ///     Set Quality of Service Level for MQTT message
    /// </summary>
    MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; }

    /// <summary>
    ///     Create an entity in Home Assistant via MQTT
    /// </summary>
    Task CreateAsync(string entityId, EntityCreationOptions? options = null, object? additionalConfig = null);

    /// <summary>
    ///     Remove an entity from Home Assistant
    /// </summary>
    Task RemoveAsync(string entityId);

    /// <summary>
    ///     Set attributes on an entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
    Task SetAttributesAsync(string entityId, object attributes);

    /// <summary>
    ///     Set availability of the entity. If you specified "payload_available" and "payload_not_available" configuration
    ///     on creating the entity then the value should match one of these.
    ///     If not, then use "online" and "offline"
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="availability"></param>
    /// <returns></returns>
    Task SetAvailabilityAsync(string entityId, string availability);

    /// <summary>
    ///     Set the state of an entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    Task SetStateAsync(string entityId, string state);

    /// <summary>
    ///    Subscribe to the entity's command topic
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    Task<IObservable<string>> SubscribeEntityCommandAsync(string entityId);
}