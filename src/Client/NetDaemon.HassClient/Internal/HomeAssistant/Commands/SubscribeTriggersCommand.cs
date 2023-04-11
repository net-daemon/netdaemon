namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record SubscribeTriggersCommand : CommandMessage
{
    public SubscribeTriggersCommand(object trigger)
    {
        Type = "subscribe_trigger";
        Trigger = trigger;
    }

    [JsonPropertyName("trigger")] 
    public object Trigger { get; init; }
}