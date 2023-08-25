namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Provides Extension methods for Entities
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Checks if an EntityState has the state "on" 
    /// </summary>
    /// <param name="entityState">The state to check</param>
    /// <returns>true if the state equals "on", otherwise false</returns>
    public static bool IsOn([NotNullWhen(true)] this EntityState? entityState) => string.Equals(entityState?.State, "on", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if an EntityState has the state "off" 
    /// </summary>
    /// <param name="entityState">The state to check</param>
    /// <returns>true if the state equals "off", otherwise false</returns>
    public static bool IsOff([NotNullWhen(true)] this EntityState? entityState) => string.Equals(entityState?.State, "off", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if an Entity has the state "on" 
    /// </summary>
    /// <param name="entity">The state to check</param>
    /// <returns>true if the state equals "on", otherwise false</returns>
    public static bool IsOn([NotNullWhen(true)] this Entity? entity) => entity?.EntityState?.IsOn() ?? false;

    /// <summary>
    /// Checks if an Entity has the state "off" 
    /// </summary>
    /// <param name="entity">The state to check</param>
    /// <returns>true if the state equals "off", otherwise false</returns>
    public static bool IsOff([NotNullWhen(true)] this Entity? entity) => entity?.EntityState?.IsOff() ?? false;

    /// <summary>Gets a NumericEntity from a given Entity</summary>
    public static NumericEntity AsNumeric(this Entity entity) => new(entity);

    /// <summary>Gets a NumericEntity from a given Entity</summary>
    public static NumericEntity<TAttributes> 
        AsNumeric<TEntity, TEntityState, TAttributes>(this Entity<TEntity, TEntityState, TAttributes> entity)
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
        => new(entity);

    /// <summary>Gets a new Entity from this Entity with the specified type of attributes</summary>
    public static Entity<TAttributes> WithAttributesAs<TAttributes>(this Entity entity)
        where TAttributes : class
        => new(entity);

    /// <summary>Gets a new Entity from this Entity with the specified type of attributes</summary>
    public static NumericEntity<TAttributes> WithAttributesAs<TAttributes>(this NumericEntity entity)
        where TAttributes : class
        => new (entity);

    internal static IObservable<T> StateChangesOnly<T>(this IObservable<T> changes) where T : StateChange
        => changes.Where(c => c.New?.State != c.Old?.State);
    
    /// <summary>  
    /// Calls a service using this entity as the target
    /// </summary> 
    /// <param name="entity">The Entity to call the service for</param>
    /// <param name="service">Name of the service to call. If the Domain of the service is the same as the domain of the Entity it can be omitted</param>
    /// <param name="data">Data to provide</param>
    public static void CallService(this IEntityCore entity, string service, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(service);

        var (serviceDomain, serviceName) = service.SplitAtDot();

        serviceDomain ??= entity.EntityId.SplitAtDot().Left ?? throw new InvalidOperationException("EntityId must be formatted 'domain.name'");

        entity.HaContext.CallService(serviceDomain, serviceName, ServiceTarget.FromEntity(entity.EntityId), data);
    }
}
