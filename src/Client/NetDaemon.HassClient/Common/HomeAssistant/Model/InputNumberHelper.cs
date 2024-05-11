namespace NetDaemon.Client.HomeAssistant.Model;

public record InputNumberHelper
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("icon")] public string? Icon { get; init; }
    [JsonPropertyName("id")] public string Id { get; init; } = string.Empty;
    [JsonPropertyName("min")] public double Min { get; init; }
    [JsonPropertyName("max")] public double Max { get; init; }
    [JsonPropertyName("step")] public double? Step { get; init; }
    [JsonPropertyName("initial")] public double? Initial { get; init; }
    [JsonPropertyName("mode")] public string? Mode { get; init; }
    [JsonPropertyName("unit_of_measurement")] public string? UnitOfMeasurement { get; init; }
}
