namespace NetDaemon.Client.HomeAssistant.Model;

public record HassEntity
{
    [JsonPropertyName("device_id")] public string? DeviceId { get; init; }

    [JsonPropertyName("entity_id")] public string? EntityId { get; init; }

    [JsonPropertyName("area_id")] public string? AreaId { get; init; }

    [JsonPropertyName("name")] public string? Name { get; init; }

    [JsonPropertyName("icon")] public string? Icon { get; init; }

    [JsonPropertyName("platform")] public string? Platform { get; init; }

    [JsonPropertyName("labels")] public IReadOnlyList<string> Labels { get; init; } = [];
}
