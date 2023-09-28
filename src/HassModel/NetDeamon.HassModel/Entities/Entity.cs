namespace NetDaemon.HassModel.Entities;

/// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
public record Entity : IEntityCore
{
    /// <inheritdoc />
    public IHaContext HaContext { get; }

    /// <inheritdoc />
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
        
    /// <inheritdoc />
    public string? Area => HaContext.GetAreaFromEntityId(EntityId)?.Name;

    /// <inheritdoc />
    public string? State => EntityState?.State;

    /// <inheritdoc />
    public virtual object? Attributes => EntityState?.Attributes;

    /// <inheritdoc />
    public virtual EntityState? EntityState => HaContext.GetState(EntityId);

    /// <inheritdoc />
    public virtual IObservable<StateChange> StateAllChanges() =>
        HaContext.StateAllChanges().Where(e => e.Entity.EntityId == EntityId);

    /// <inheritdoc />
    public virtual IObservable<StateChange> StateChanges() =>
        StateAllChanges().StateChangesOnly();

    /// <inheritdoc />
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