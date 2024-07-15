namespace NetDaemon.Tests.Performance;

public record CommandMessage
{
    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;
    [JsonPropertyName("id")] public int Id { get; set; }
}

internal record CreateInputBooleanCommand : CommandMessage
{
    public CreateInputBooleanCommand()
    {
        Type = "input_boolean/create";
    }

    [JsonPropertyName("name")] public required string Name { get; init; }
}
