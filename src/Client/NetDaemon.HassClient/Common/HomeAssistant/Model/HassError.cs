namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record HassError
{
    [JsonPropertyName("code")] public object? Code { get; init; }

    [JsonPropertyName("message")] public string? Message { get; init; }
}