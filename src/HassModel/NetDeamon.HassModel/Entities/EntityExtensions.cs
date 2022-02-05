namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Provides Extension methods for Entities
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Checks if en EntityState has the state "on" 
    /// </summary>
    /// <param name="entityState">The state to check</param>
    /// <returns>true if the state equals "on", otherwise false</returns>
    public static bool IsOn(this EntityState? entityState) => string.Equals(entityState?.State, "on", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if en EntityState has the state "off" 
    /// </summary>
    /// <param name="entityState">The state to check</param>
    /// <returns>true if the state equals "off", otherwise false</returns>
    public static bool IsOff(this EntityState? entityState) => string.Equals(entityState?.State, "off", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if en Entity has the state "on" 
    /// </summary>
    /// <param name="entity">The state to check</param>
    /// <returns>true if the state equals "on", otherwise false</returns>
    public static bool IsOn(this Entity? entity) => entity?.EntityState?.IsOn() ?? false;

    /// <summary>
    /// Checks if en Entity has the state "off" 
    /// </summary>
    /// <param name="entity">The state to check</param>
    /// <returns>true if the state equals "off", otherwise false</returns>
    public static bool IsOff(this Entity? entity) => entity?.EntityState?.IsOff() ?? false;

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
    
    public static IObservable<StateChange> SameStateFor(this IObservable<StateChange> observable, Func<EntityState?, bool> predicate, TimeSpan timeSpan) =>
        observable
            .Where(e => predicate(e.Old) != predicate(e.New))
            .Throttle(timeSpan)
            .Where(e => predicate(e.New));
    
    /// <summary>
    /// Observable, All state changes. New.State != Old.State
    /// </summary>
    public static IObservable<StateChange<TEntity, TEntityState>> SameStateFor<TEntity, TEntityState>(this IObservable<StateChange<TEntity, TEntityState>> observable, Func<TEntityState?, bool> predicate, TimeSpan timeSpan) 
        where TEntity : Entity
        where TEntityState : EntityState =>
        observable
            .Where(e => predicate(e.Old) != predicate(e.New))
            .Throttle(timeSpan)
            .Where(e => predicate(e.New));
}