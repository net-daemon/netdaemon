namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record TimePatternTrigger : TriggerBase
{
    public TimePatternTrigger()
    {
        Platform = "time_pattern";
    }
    
    [JsonPropertyName("seconds")] public string? Seconds { get; init; }
    [JsonPropertyName("minutes")] public string? Minutes { get; init; }
    [JsonPropertyName("hours")] public string? Hours { get; init; }
}