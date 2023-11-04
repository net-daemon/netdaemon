namespace NetDaemon.HassModel;

/// <summary>
/// Extension methods for HaContext
/// </summary>
public static class HaContextExtensions
{
    /// <summary>
    /// The observable state stream state change
    /// </summary>
    /// <remarks>
    /// Old state != New state
    /// </remarks>
    public static IObservable<StateChange> StateChanges(this IHaContext haContext)
    {
        ArgumentNullException.ThrowIfNull(haContext, nameof(haContext));
        return haContext.StateAllChanges().StateChangesOnly();
    }

    /// <summary>
    /// Creates a new Entity instance
    /// </summary>
    public static Entity Entity(this IHaContext haContext, string entityId) => new (haContext, entityId);

    /// <summary>
    /// Filters events on their EventType and retrieves their data in a types object
    /// </summary>
    /// <param name="events">The Event stream</param>
    /// <param name="eventType">The event_type to filter on</param>
    /// <typeparam name="T">Type to deserialize of the data json element</typeparam>
    /// <returns>Observable of matching events with deserialized data</returns>
    public static IObservable<Event<T>> Filter<T>(this IObservable<Event> events, string eventType)
        where T : class
        => events
            .Where(e => e.EventType == eventType && e.DataElement != null)
            .Select(e => new Event<T>(e));
}
