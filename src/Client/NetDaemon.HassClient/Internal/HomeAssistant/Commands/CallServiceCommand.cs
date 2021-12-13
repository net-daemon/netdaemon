namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record CallServiceCommand : CommandMessage
{
    public CallServiceCommand() => Type = "call_service";

    [JsonPropertyName("domain")]
    public string Domain { get; init; } = string.Empty;

    [JsonPropertyName("service")]
    public string Service { get; init; } = string.Empty;

    [JsonPropertyName("service_data")]
    public object? ServiceData { get; init; }

    [JsonPropertyName("target")]
    public HassTarget? Target { get; init; }
}