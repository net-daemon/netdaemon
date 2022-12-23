namespace NetDaemon.HassModel;

/// <summary>
/// Represents a context for interacting with Home Assistant
/// </summary>
public interface IHaContext
{
    /// <summary>
    /// All Events from Home Assistant 
    /// </summary>
    IObservable<Event> Events { get; }

    /// <summary>
    ///     The observable state stream, all changes including attributes
    /// </summary>
    IObservable<StateChange> StateAllChanges();

    /// <summary>
    /// The observable state stream, all changes including attributes
    /// </summary>
    /// <returns></returns>
    IObservable<IStateChange> StateAllChangesGeneric();

    /// <summary>
    /// Get state for a single entity
    /// </summary>
    /// <param name="entityId"></param>
    EntityState? GetState(string entityId);

    /// <summary>
    /// Get state for a single entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    IEntityState? GetStateGeneric(string entityId);

    /// <summary>
    /// Gets all the entities in HomeAssistant
    /// </summary>
    IReadOnlyList<Entity> GetAllEntities();

    /// <summary>
    /// Gets all the entities in HomeAssistant
    /// </summary>
    /// <returns></returns>
    IReadOnlyList<IEntity> GetAllEntitiesGeneric();

    /// <summary>
    /// Calls a service in Home Assistant
    /// </summary>
    /// <param name="domain">Domain of service</param>
    /// <param name="service">Service name</param>
    /// <param name="target">The target that is targeted by this service call</param>
    /// <param name="data">Data provided to service. Should be Json-serializable to the data expected by the service</param>
    void CallService(string domain, string service, ServiceTarget? target = null, object? data = null);

    /// <summary>
    /// Get area for a single entity
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    Area? GetAreaFromEntityId(string entityId);

    /// <summary>
    /// Sends an event to Home Assistant
    /// </summary>
    /// <param name="eventType">The event_type for the event</param>
    /// <param name="data">The data for the event, will be json serialized as the date element</param>
    void SendEvent(string eventType, object? data = null);
}