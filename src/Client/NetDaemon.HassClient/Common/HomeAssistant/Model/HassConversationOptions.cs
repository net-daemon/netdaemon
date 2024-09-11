namespace NetDaemon.Client.HomeAssistant.Model
{
    public record HassEntityConversationOptions
    {
        [JsonPropertyName("should_expose")] public bool ShouldExpose { get; init; }
    }
}
