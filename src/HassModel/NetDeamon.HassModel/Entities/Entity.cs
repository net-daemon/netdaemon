namespace NetDaemon.HassModel.Entities;

/// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
public record Entity : IEntityCore
{
    /// <summary>
    /// The IHAContext
    /// </summary>
    public IHaContext HaContext { get; }

    /// <summary>
    /// Entity id being handled by this entity
    /// </summary>
    public string EntityId { get; }

    /// <summary>
    /// Creates a new instance of a Entity class
    /// </summary>
    /// <param name="haContext">The Home Assistant context associated with this Entity</param>
    /// <param name="entityId">The id of this Entity</param>
    public Entity(IHaContext haContext, string entityId)
    {
        HaContext = haContext;
        EntityId = entityId;
    }

    /// <summary>Copy constructor from IEntityCore</summary>
    public Entity(IEntityCore entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        HaContext = entity.HaContext;
        EntityId = entity.EntityId;
    }
        
    /// <summary>
    /// Area name of entity
    /// </summary>
    public string? Area => HaContext.GetAreaFromEntityId(EntityId)?.Name;

    /// <summary>The current state of this Entity</summary>
    public string? State => EntityState?.State;

    /// <summary>
    /// The current Attributes of this Entity
    /// </summary>
    public virtual object? Attributes => EntityState?.Attributes;

    /// <summary>
    /// The full state of this Entity
    /// </summary>
    public virtual EntityState? EntityState => HaContext.GetState(EntityId);

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
    public virtual IObservable<StateChange> StateAllChanges() =>
        HaContext.StateAllChanges().Where(e => e.Entity.EntityId == EntityId);

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
    public virtual IObservable<StateChange> StateChanges() =>
        StateAllChanges().StateChangesOnly();

    /// <summary>
    /// Calls a service using this entity as the target
    /// </summary>
    /// <param name="service">Name of the service to call. If the Domain of the service is the same as the domain of the Entity it can be omitted</param>
    /// <param name="data">Data to provide</param>
    public virtual void CallService(string service, object? data = null)
    {
        EntityExtensions.CallService(this, service, data);
    }
}

/// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
public abstract record Entity<TEntity, TEntityState, TAttributes> : Entity
    where TEntity : Entity<TEntity, TEntityState, TAttributes>
    where TEntityState : EntityState<TAttributes>
    where TAttributes : class
{
    /// <summary>Copy constructor from IEntityCore</summary>
    protected Entity(IEntityCore entity) : base(entity)
    { }

    /// <summary>Constructor from haContext and entityId</summary>
    protected Entity(IHaContext haContext, string entityId) : base(haContext, entityId)
    { }

    /// <inheritdoc />
    public override TAttributes? Attributes => EntityState?.Attributes;

    /// <inheritdoc />
    public override TEntityState? EntityState => MapState(base.EntityState);

    /// <inheritdoc />
    public override IObservable<StateChange<TEntity, TEntityState>> StateAllChanges() =>
        base.StateAllChanges().Select(e => new StateChange<TEntity, TEntityState>((TEntity)this, 
            Entities.EntityState.Map<TEntityState>(e.Old), 
            Entities.EntityState.Map<TEntityState>(e.New)));

    /// <inheritdoc />
    public override IObservable<StateChange<TEntity, TEntityState>> StateChanges() => StateAllChanges().StateChangesOnly();

    private static TEntityState? MapState(EntityState? state) => Entities.EntityState.Map<TEntityState>(state);
}
    
/// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
public record Entity<TAttributes> : Entity<Entity<TAttributes>, EntityState<TAttributes>, TAttributes>
    where TAttributes : class
{
    // This type is needed because the base type has a recursive type parameter so it can not be used as a return value
        
    /// <summary>Copy constructor from IEntityCore</summary>
    public Entity(IEntityCore entity) : base(entity) { }
        
    /// <summary>Constructor from haContext and entityId</summary>
    public Entity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
}