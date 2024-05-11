namespace NetDaemon.Client.Internal.HomeAssistant.Commands;
// {"type":"input_number/create","min":2,"max":100,"name":"Hello","icon":"mdi:account","id":45}
internal record CreateInputNumberHelperCommand : CommandMessage
{
    public CreateInputNumberHelperCommand()
    {
        Type = "input_number/create";
    }

    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("icon")] public string? Icon { get; init; }
    [JsonPropertyName("min")] public double Min { get; init; }
    [JsonPropertyName("max")] public double Max { get; init; }
    [JsonPropertyName("step")] public double? Step { get; init; }
    [JsonPropertyName("initial")] public double? Initial { get; init; }
    [JsonPropertyName("mode")] public string? Mode { get; init; }
    [JsonPropertyName("unit_of_measurement")] public string? UnitOfMeasurement { get; init; }
}

internal record DeleteInputNumberHelperCommand : CommandMessage
{
    public DeleteInputNumberHelperCommand()
    {
        Type = "input_number/delete";
    }

    [JsonPropertyName("input_number_id")] public required string InputNumberId { get; init; } = string.Empty;
}

internal record ListInputNumberHelperCommand : CommandMessage
{
    public ListInputNumberHelperCommand()
    {
        Type = "input_number/list";
    }
}
