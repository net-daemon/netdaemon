namespace NetDaemon.Client.HomeAssistant.Model;

public record HassArea
{
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("area_id")] public string? Id { get; init; }

    [JsonPropertyName("labels")] public IReadOnlyList<string> Labels { get; init; } = [];

    [JsonPropertyName("floor_id")] public string? FloorId { get; init; }

    [JsonPropertyName("icon")] public string? Icon { get; init; }
}
