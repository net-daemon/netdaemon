using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record SubscribeTriggersCommand<T> : CommandMessage where T: TriggerBase
{
    public SubscribeTriggersCommand(T trigger)
    {
        Type = "subscribe_trigger";
        Trigger = trigger;
    }

    [JsonPropertyName("trigger")] public T Trigger { get; init; }
}