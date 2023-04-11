namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record UnsubscribeTriggersCommand : CommandMessage
{
    public UnsubscribeTriggersCommand(int subscriptionId)
    {
        Type = "unsubscribe_events";
        Subscription = subscriptionId;
    }

    [JsonPropertyName("subscription")] public int Subscription { get; init; }
}