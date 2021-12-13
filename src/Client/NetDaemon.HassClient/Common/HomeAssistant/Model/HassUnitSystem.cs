namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record HassUnitSystem
{
    [JsonPropertyName("length")]
    public string? Length { get; init; }

    [JsonPropertyName("mass")]
    public string? Mass { get; init; }

    [JsonPropertyName("temperature")]
    public string? Temperature { get; init; }

    [JsonPropertyName("volume")]
    public string? Volume { get; init; }
}
