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
    /// <param name="entityId">Distinct identifier, in the format "domain.id", such as "sensor.kitchen_temp".</param>
    /// <param name="options">Optional entity creation options.</param>
    /// <param name="additionalConfig">Optional additional Home Assistant MQTT discovery configuration.</param>
    /// <returns>A task that represents the asynchronous create operation.</returns>
    Task CreateAsync(string entityId, EntityCreationOptions? options = null, object? additionalConfig = null);

    /// <summary>
    ///     Remove an entity from Home Assistant
    /// </summary>
    /// <param name="entityId">The entity id.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    Task RemoveAsync(string entityId);

    /// <summary>
    ///     Set attributes on an entity
    /// </summary>
    /// <param name="entityId">The entity id.</param>
    /// <param name="attributes">The attributes to set.</param>
    /// <returns>A task that represents the asynchronous attribute update operation.</returns>
    Task SetAttributesAsync(string entityId, object attributes);

    /// <summary>
    ///     Set availability of the entity. If you specified "payload_available" and "payload_not_available" configuration
    ///     on creating the entity then the value should match one of these.
    ///     If not, then use "online" and "offline"
    /// </summary>
    /// <param name="entityId">The entity id.</param>
    /// <param name="availability">The availability payload.</param>
    /// <returns>A task that represents the asynchronous availability update operation.</returns>
    Task SetAvailabilityAsync(string entityId, string availability);

    /// <summary>
    ///     Set the state of an entity
    /// </summary>
    /// <param name="entityId">The entity id.</param>
    /// <param name="state">The state payload.</param>
    /// <returns>A task that represents the asynchronous state update operation.</returns>
    Task SetStateAsync(string entityId, string state);

    /// <summary>
    /// Prepare a subscription to command topics for the given entity
    /// <para>Be sure to chain this request with .Subscribe(...)</para>
    /// </summary>
    /// <param name="entityId">The entity id.</param>
    /// <returns>An observable that receives command payloads for the entity.</returns>
    Task<IObservable<string>> PrepareCommandSubscriptionAsync(string entityId);
}
