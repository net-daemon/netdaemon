namespace NetDaemon.HassModel;

public static class TriggerManagerExtensions
{
    /// <summary>
    /// Registers a trigger in HA and returns an Observable with the events
    /// </summary>
    /// <param name="triggerParams">Input data for HA register_trigger command</param>
    /// <typeparam name="T">The type to deserialize the resulting messages to</typeparam>
    /// <returns>IObservable with all events resulting from this trigger</returns>
    public static IObservable<T?> RegisterTrigger<T>(this ITriggerManager triggerManager, object triggerParams) => triggerManager.RegisterTrigger(triggerParams).Select(m => m.Deserialize<T>());
}