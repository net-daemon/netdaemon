namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record UnsubscribeEventsCommand : CommandMessage
{
    public UnsubscribeEventsCommand(int subscriptionId)
    {
        Type = "unsubscribe_events";
        Subscription = subscriptionId;
    }

    [JsonPropertyName("subscription")] public int Subscription { get; init; }
}