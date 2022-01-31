namespace NetDaemon.Client.HomeAssistant.Model;

public record HassServiceEventData
{
    [JsonPropertyName("domain")] public string Domain { get; init; } = string.Empty;

    [JsonPropertyName("service")] public string Service { get; init; } = string.Empty;

    [JsonPropertyName("service_data")] public JsonElement? ServiceDataElement { get; init; }
}