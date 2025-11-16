namespace NetDaemon.Client.HomeAssistant.Model;

public record HassFloor
{
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("level")]
    public short? Level { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("floor_id")]
    public string? Id { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
