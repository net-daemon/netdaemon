using NetDaemon.Extensions.MqttEntityManager.Models;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
///     Interface for managing entities via MQTT
/// </summary>
public interface IMqttEntityManager
{
    /// <summary>
    ///     Create an entity in Home Assistant via MQTT
    /// </summary>
    Task CreateAsync(string entityId, EntityCreationOptions? options = null);

    /// <summary>
    ///     Remove an entity from Home Assistant
    /// </summary>
    Task RemoveAsync(string entityId);

    /// <summary>
    ///     Update state and/or set attributes of an HA entity via MQTT
    /// </summary>
    Task UpdateAsync(string entityId, object? state, object? attributes = null);

    /// <summary>
    /// Set availability of the entity. If you specified "payload_available" and "payload_not_available" configuration
    /// on creating the entity then the value should match one of these.
    /// If not, then use "online" and "offline"
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="availability"></param>
    /// <returns></returns>
    Task SetAvailabilityAsync(string entityId, string availability);
}