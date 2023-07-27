namespace NetDaemon.Client.HomeAssistant.Model;

public record HassServiceResult
{
    [JsonPropertyName("context")] public HassContext? Context { get; init; }
    [JsonPropertyName("response")] public JsonElement? Response { get; init; }
}