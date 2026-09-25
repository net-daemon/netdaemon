namespace NetDaemon.Client.HomeAssistant.Model;

public record HassEntity
{
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("entity_id")]
    public string? EntityId { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("area_id")]
    public string? AreaId { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("platform")]
    public string? Platform { get; init; }

    [JsonConverter(typeof(EnsureArrayOfStringConverter))]
    [JsonPropertyName("labels")]
    public IReadOnlyList<string> Labels { get; init; } = [];

    [JsonPropertyName("options")]
    public HassEntityOptions? Options { get; init; }
}
