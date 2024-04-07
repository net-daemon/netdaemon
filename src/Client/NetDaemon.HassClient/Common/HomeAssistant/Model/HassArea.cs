namespace NetDaemon.Client.HomeAssistant.Model;

public record HassArea
{
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("area_id")] public string? Id { get; init; }

    [JsonPropertyName("labels")] public IReadOnlyList<string> Labels { get; init; }

}
