namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record HassEvent
{
    [JsonPropertyName("data")] public JsonElement? DataElement { get; init; }

    [JsonPropertyName("event_type")] public string EventType { get; init; } = string.Empty;

    [JsonPropertyName("origin")] public string Origin { get; init; } = string.Empty;

    [JsonPropertyName("time_fired")] public DateTime? TimeFired { get; init; }
}
