namespace NetDaemon.Client.HomeAssistant.Extensions;

/// <summary>
///     Extensions to HassEvents to convert to more specific event types
/// </summary>
public static class HassEventExtensions
{
    /// <summary>
    ///     Convert a HassEvent to a StateChangedEvent
    /// </summary>
    /// <param name="hassEvent">HassEvent to convert</param>
    /// <exception cref="NullReferenceException"></exception>
    public static HassStateChangedEventData? ToStateChangedEvent(this HassEvent hassEvent)
    {
        var jsonElement = hassEvent.DataElement ??
                          throw new NullReferenceException("DataElement cannot be empty");
        return jsonElement.Deserialize<HassStateChangedEventData>();
    }
    
    /// <summary>
    ///     Convert a HassEvent to CallServiceEvent
    /// </summary>
    /// <param name="hassEvent">HassEvent to convert</param>
    /// <exception cref="NullReferenceException"></exception>
    public static HassServiceEventData? ToCallServiceEvent(this HassEvent hassEvent)
    {
        var jsonElement = hassEvent.DataElement ??
                          throw new NullReferenceException("DataElement cannot be empty");
        return jsonElement.Deserialize<HassServiceEventData>();
    }
}