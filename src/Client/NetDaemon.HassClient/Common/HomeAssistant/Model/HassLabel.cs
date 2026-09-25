namespace NetDaemon.Client.HomeAssistant.Model;

public record HassLabel
{
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("color")]
    public string? Color { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("label_id")]
    public string? Id { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
