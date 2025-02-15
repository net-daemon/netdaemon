namespace NetDaemon.HassModel;

/// <summary>
/// Enables the creation of triggers
/// </summary>
public interface ITriggerManager
{
    /// <summary>
    /// Registers a trigger in HA and returns an Observable with the events
    /// </summary>
    /// <param name="triggerParams">Input data for HA register_trigger command</param>
    /// <returns>IObservable with all events resulting from this trigger</returns>
    IObservable<JsonElement> RegisterTrigger(object triggerParams);
}