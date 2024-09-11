namespace NetDaemon.Client.HomeAssistant.Model
{
    public record HassEntityOptions
    {
        [JsonPropertyName("conversation")] public HassEntityConversationOptions? Conversation { get; init; }
    }
}
