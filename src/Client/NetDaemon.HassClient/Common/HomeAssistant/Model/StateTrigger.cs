namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record StateTrigger : TriggerBase
{
    public StateTrigger()
    {
        Platform = "state";
    }
    
    [JsonPropertyName("entity_id")] public string[] EntityId { get; init; } = Array.Empty<string>();
    
    [JsonPropertyName("attribute")] public string? Attribute { get; init; }
    
    [JsonPropertyName("from")] public string[]? From { get; init; }
    [JsonPropertyName("to")] public string[]? To { get; init; }
    
    [JsonPropertyName("not_from")] public string[]? NotFrom { get; init; }
    [JsonPropertyName("not_to")] public string[]? NotTo { get; init; }
    
    // For support not added yet...
}