namespace NetDaemon.Client.HomeAssistant.Model;

/// <summary>
///     Represents a target for a service call in Home Assistant
/// </summary>
public record HassTarget
{
    /// <summary>
    ///     Zero or more entity ids to target with the service call
    /// </summary>
    [JsonPropertyName("entity_id")]
    public IReadOnlyCollection<string>? EntityIds { get; init; }

    /// <summary>
    ///     Zero or more device ids to target with the service call
    /// </summary>
    [JsonPropertyName("device_id")]
    public IReadOnlyCollection<string>? DeviceIds { get; init; }

    /// <summary>
    ///     Zero or more area ids to target with the service call
    /// </summary>
    [JsonPropertyName("area_id")]
    public IReadOnlyCollection<string>? AreaIds { get; init; }
}