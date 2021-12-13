namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record HassState
{
    [JsonPropertyName("attributes")] public JsonElement? AttributesJson { get; set; }

    public IReadOnlyDictionary<string, object>? Attributes
    {
        get => AttributesJson?.ToObject<Dictionary<string, object>>() ?? new();
        init => AttributesJson = value.ToJsonElement();
    }

    public T? AttributesAs<T>() => AttributesJson.HasValue ? AttributesJson.Value.ToObject<T>() : default;

    [JsonPropertyName("entity_id")] public string EntityId { get; init; } = "";

    [JsonPropertyName("last_changed")] public DateTime LastChanged { get; init; } = DateTime.MinValue;
    [JsonPropertyName("last_updated")] public DateTime LastUpdated { get; init; } = DateTime.MinValue;
    [JsonPropertyName("state")] public string? State { get; init; } = "";
    [JsonPropertyName("context")] public HassContext? Context { get; init; }
}
