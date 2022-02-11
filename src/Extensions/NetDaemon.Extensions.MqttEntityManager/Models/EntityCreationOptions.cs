namespace NetDaemon.Extensions.MqttEntityManager.Models;

/// <summary>
/// Parameters to create an entity
/// </summary>
/// <param name="DeviceClass">Optional device class - see HA integration documentation</param>
/// <param name="UniqueId">Optional unique ID to use - if not specified then one will be generated</param>
/// <param name="Name">Optional name of the entity</param>
/// <param name="Persist">Optionally persist the entity over HA restarts, default is true</param>
/// <param name="QualityOfService">Optional MQTT QOS level, default is 0</param>
/// <param name="AdditionalOptions">Optional additional parameters to set</param>
public record EntityCreationOptions(
    string? DeviceClass = null,
    string? UniqueId = null,
    string? Name = null,
    bool Persist = true,
    int QualityOfService = 0,
    dynamic? AdditionalOptions = null
);