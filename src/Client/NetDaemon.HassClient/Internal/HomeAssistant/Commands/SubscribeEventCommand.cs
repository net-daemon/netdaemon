namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record SubscribeEventCommand : CommandMessage
{
    public SubscribeEventCommand()
    {
        Type = "subscribe_events";
    }

    [JsonPropertyName("event_type")] public string? EventType { get; init; }
}