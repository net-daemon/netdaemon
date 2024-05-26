namespace NetDaemon.Client.HomeAssistant.Model;

public record InputBooleanHelper
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("icon")] public string? Icon { get; init; }
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
}
