namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Interface for managing entities via MQTT
/// </summary>
public interface IMqttEntityManager
{
 /// <summary>
    /// Create an entity in Home Assistant via MQTT
    /// </summary>
    Task CreateAsync(string domain, string entityId, string deviceClass, string name);
   
    
    /// <summary>
    /// Update state and, optionally, attributes of an HA entity via MQTT
    /// </summary>
    
    /// <summary>
    /// Remove an entity from Home Assistant
    /// </summary>
    Task RemoveAsync(string domain, string entityId);
    Task UpdateAsync(string domain, string entityId, string state, string? attributes = null);
}