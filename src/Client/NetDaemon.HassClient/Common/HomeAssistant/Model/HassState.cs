namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record HassState
{
    [JsonPropertyName("attributes")] public JsonElement? AttributesJson { get; set; }

    public IReadOnlyDictionary<string, object>? Attributes
    {
        get => AttributesJson?.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
        init => AttributesJson = value.ToJsonElement();
    }

    [JsonPropertyName("entity_id")] public string EntityId { get; init; } = "";

    [JsonPropertyName("last_changed")] public DateTime LastChanged { get; init; } = DateTime.MinValue;
    [JsonPropertyName("last_updated")] public DateTime LastUpdated { get; init; } = DateTime.MinValue;
    [JsonPropertyName("state")] public string? State { get; init; } = "";
    [JsonPropertyName("context")] public HassContext? Context { get; init; }

    public T? AttributesAs<T>()
    {
        return AttributesJson.HasValue ? AttributesJson.Value.ToObject<T>() : default;
    }
}