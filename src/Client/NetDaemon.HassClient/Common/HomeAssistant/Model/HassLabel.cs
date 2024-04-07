namespace NetDaemon.Client.HomeAssistant.Model;

public record HassLabel
{
    [JsonPropertyName("color")] public string? Color { get; init; }

    [JsonPropertyName("description")] public string? Description { get; init; }

    [JsonPropertyName("icon")] public string? Icon { get; init; }

    [JsonPropertyName("label_id")] public string? Id { get; init; }

    [JsonPropertyName("name")] public string? Name { get; init; }
}
