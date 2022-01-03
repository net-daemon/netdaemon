namespace NetDaemon.Client.Common.HomeAssistant.Extensions;

public static class HassEventExtensions
{
    public static HassStateChangedEventData? ToStateChangedEvent(this HassEvent hassEvent)
    {
        var jsonElement = hassEvent.DataElement ??
                          throw new NullReferenceException("DataElement cannot be empty");
        return jsonElement.ToObject<HassStateChangedEventData>();
    }

    public static HassServiceEventData? ToCallServiceEvent(this HassEvent hassEvent)
    {
        var jsonElement = hassEvent.DataElement ??
                          throw new NullReferenceException("DataElement cannot be empty");
        return jsonElement.ToObject<HassServiceEventData>();
    }
}