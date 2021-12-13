namespace NetDaemon.Client.Common.HomeAssistant.Model;
public record HassMessageBase
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;
}