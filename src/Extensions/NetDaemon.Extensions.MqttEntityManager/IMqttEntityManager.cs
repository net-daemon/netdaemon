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
    ///     Update state and, optionally, attributes of an HA entity via MQTT
    /// </summary>
    Task UpdateAsync(string entityId, string state, string? attributes = null);
    
    /// <summary>
    ///     Update state and, optionally, attributes of an HA entity via MQTT
    /// </summary>
    Task UpdateAsync(string entityId, string state, object? attributes = null);
}