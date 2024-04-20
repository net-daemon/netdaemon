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
        AsNumeric<TEntityState, TAttributes>(this Entity<TEntityState, TAttributes> entity)
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
    /// Observable that emits all state changes, including attribute changes.<br/>
    /// Use <see cref="System.ObservableExtensions.Subscribe{T}(System.IObservable{T})"/> to subscribe to the returned observable and receive state changes.
    /// </summary>
    /// <example>
    /// <code>
    /// bedroomLight.StateAllChanges()
    ///     .Where(s =&gt; s.Old?.Attributes?.Brightness &lt; 128
    ///              &amp;&amp; s.New?.Attributes?.Brightness &gt;= 128)
    ///     .Subscribe(e =&gt; HandleBrightnessOverHalf());
    /// </code>
    /// </example>
    public static IObservable<StateChange> StateAllChanges(this Entity entity) =>
        entity.HaContext.StateAllChanges().Where(e => e.Entity.EntityId == entity.EntityId);

    /// <summary>
    /// Observable that emits state changes where New.State != Old.State<br/>
    /// Use <see cref="System.ObservableExtensions.Subscribe{T}(System.IObservable{T})"/> to subscribe to the returned observable and receive state changes.
    /// </summary>
    /// <example>
    /// <code>
    /// disabledLight.StateChanges()
    ///    .Where(s =&gt; s.New?.State == "on")
    ///    .Subscribe(e =&gt; e.Entity.TurnOff());
    /// </code>
    /// </example>
    public static IObservable<StateChange> StateChanges(this Entity entity) =>
        entity.StateAllChanges().StateChangesOnly();

    public static IObservable<NumericStateChange> StateChanges(this NumericEntity entity) =>
        entity.StateAllChanges().StateChangesOnly();

    public static IObservable<NumericStateChange> StateAllChanges(this NumericEntity entity) =>
        ((Entity)entity).StateAllChanges().Select(e => new NumericStateChange(entity,
            Entities.EntityState.Map<NumericEntityState>(e.Old),
            Entities.EntityState.Map<NumericEntityState>(e.New)));

    /// <inheritdoc />
    public static IObservable<StateChange<Entity<TEntityState, TAttributes>, TEntityState>> StateAllChanges<TEntityState, TAttributes>(this Entity<TEntityState, TAttributes> entity)
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
        =>
            ((Entity)entity).StateAllChanges().Select(e => new StateChange<Entity<TEntityState, TAttributes>, TEntityState>(entity,
            Entities.EntityState.Map<TEntityState>(e.Old),
            Entities.EntityState.Map<TEntityState>(e.New)));

    /// <inheritdoc />
    public static IObservable<StateChange<Entity<TEntityState, TAttributes>, TEntityState>> StateChanges<TEntityState, TAttributes>(this Entity<TEntityState, TAttributes> entity)
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
        => entity.StateAllChanges().StateChangesOnly();

    /// <inheritdoc />
    public static IObservable<NumericStateChange<NumericEntity<TEntityState, TAttributes>, TEntityState>> StateAllChanges<TEntityState, TAttributes>(this NumericEntity<TEntityState, TAttributes> entity)
        where TEntityState : NumericEntityState<TAttributes>
        where TAttributes : class
        => ((Entity<TEntityState, TAttributes>)entity).StateAllChanges().Select(e => new NumericStateChange<NumericEntity<TEntityState, TAttributes>, TEntityState>(entity, e.Old, e.New));

    /// <inheritdoc />
    public static IObservable<NumericStateChange<NumericEntity<TEntityState, TAttributes>, TEntityState>> StateChanges<TEntityState, TAttributes>(this NumericEntity<TEntityState, TAttributes> entity)
        where TEntityState : NumericEntityState<TAttributes>
        where TAttributes : class
        => entity.StateAllChanges().StateChangesOnly();

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
