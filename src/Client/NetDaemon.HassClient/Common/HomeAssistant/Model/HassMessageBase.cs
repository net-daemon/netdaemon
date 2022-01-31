namespace NetDaemon.Client.HomeAssistant.Model;

public record HassMessageBase
{
    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;
}