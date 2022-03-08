namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Parameters to create an entity
/// </summary>
/// <param name="DeviceClass">Optional device class - see HA integration documentation</param>
/// <param name="UniqueId">Optional unique ID to use - if not specified then one will be generated</param>
/// <param name="Name">Optional name of the entity</param>
/// <param name="PayloadAvailable">Optional payload to set the entity available</param>
/// <param name="PayloadNotAvailable">Optional payload to set the entity not-available</param>
/// <param name="PayloadOn">Optional payload to set the command on</param>
/// <param name="PayloadOff">Optional payload to set the command off</param>
/// <param name="Persist">Optionally persist the entity over HA restarts, default is true</param>
public record EntityCreationOptions(
    string? DeviceClass = null,
    string? UniqueId = null,
    string? Name = null,
    string? PayloadAvailable = null,
    string? PayloadNotAvailable = null,
    string? PayloadOn = null,
    string? PayloadOff = null,
    bool Persist = true
);