namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record SubscribeTriggerCommand : CommandMessage
{
    public SubscribeTriggerCommand(object trigger)
    {
        Type = "subscribe_trigger";
        Trigger = trigger;
    }

    [JsonPropertyName("trigger")] 
    public object Trigger { get; init; }
}