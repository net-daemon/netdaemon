using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.Client.HomeAssistant.Model;

public record HassEvent
{
    [JsonPropertyName("data")] public JsonElement? DataElement { get; init; }

    [JsonPropertyName("variables")] public HassVariable? Variables { get; init; }

    [JsonPropertyName("event_type")] public string EventType { get; init; } = string.Empty;

    [JsonPropertyName("origin")] public string Origin { get; init; } = string.Empty;

    [JsonPropertyName("time_fired")] public DateTime? TimeFired { get; init; }
}