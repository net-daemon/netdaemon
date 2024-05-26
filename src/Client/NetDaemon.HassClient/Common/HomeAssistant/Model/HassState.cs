namespace NetDaemon.Client.HomeAssistant.Model;

public record HassState
{
    [JsonPropertyName("attributes")] public JsonElement? AttributesJson { get; init; }

    public IReadOnlyDictionary<string, object>? Attributes
    {
        get => AttributesJson?.Deserialize<Dictionary<string, object>>() ?? [];
        init => AttributesJson = value.ToJsonElement();
    }

    [JsonPropertyName("entity_id")] public string EntityId { get; init; } = "";

    [JsonPropertyName("last_changed")] public DateTime LastChanged { get; init; } = DateTime.MinValue;
    [JsonPropertyName("last_updated")] public DateTime LastUpdated { get; init; } = DateTime.MinValue;
    [JsonPropertyName("state")] public string? State { get; init; } = "";
    [JsonPropertyName("context")] public HassContext? Context { get; init; }

    public T? AttributesAs<T>()
    {
        return AttributesJson.HasValue ? AttributesJson.Value.Deserialize<T>() : default;
    }
}
