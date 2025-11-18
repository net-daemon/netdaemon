namespace NetDaemon.Client.HomeAssistant.Model;

public record HassArea
{
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("area_id")]
    public string? Id { get; init; }

    [JsonConverter(typeof(EnsureArrayOfStringConverter))]
    [JsonPropertyName("labels")]
    public IReadOnlyList<string> Labels { get; init; } = [];

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("floor_id")]
    public string? FloorId { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }
}
