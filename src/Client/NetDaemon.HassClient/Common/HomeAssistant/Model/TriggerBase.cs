namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record TriggerBase   
{
    [JsonPropertyName("platform")] public string Platform { get; init; } = string.Empty;
}