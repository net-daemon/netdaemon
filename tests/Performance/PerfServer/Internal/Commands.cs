// Contains commands that being sent from ND that needs to be de-serialized by the fake sever

namespace NetDaemon.Tests.Performance;

#pragma warning disable CA1852

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

internal record CallServiceCommand : CommandMessage
{
    public CallServiceCommand()
    {
        Type = "call_service";
    }

    [JsonPropertyName("domain")] public string Domain { get; init; } = string.Empty;

    [JsonPropertyName("service")] public string Service { get; init; } = string.Empty;

    [JsonPropertyName("service_data")] public object? ServiceData { get; init; }

    [JsonPropertyName("target")] public HassTarget? Target { get; init; }
}
