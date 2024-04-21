namespace NetDaemon.HassModel.Entities;

/// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
public record Entity<TEntity, TEntityState, TAttributes, TState> : IEntityCore
    where TEntityState : EntityState<TState ,TAttributes>
    where TAttributes : class
    where TEntity : Entity<TEntity, TEntityState, TAttributes, TState>
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

    public TState? State => EntityState.State;

    /// <inheritdoc />
    public virtual TAttributes? Attributes => EntityState?.Attributes;

    /// <inheritdoc />
    public virtual TEntityState? EntityState => MapState(HaContext.GetState(EntityId));

    /// <summary>
    /// Calls a service using this entity as the target
    /// </summary>
    /// <param name="service">Name of the service to call. If the Domain of the service is the same as the domain of the Entity it can be omitted</param>
    /// <param name="data">Data to provide</param>
    public virtual void CallService(string service, object? data = null)
    {
        EntityExtensions.CallService(this, service, data);
    }

    /// <summary>Gets a NumericEntity from a given Entity</summary>
    public NumericEntity<TAttributes> AsNumeric() => new(this);

    /// <summary>Gets a new Entity from this Entity with the specified type of attributes</summary>
    public Entity<T> WithAttributesAs<T>()
        where T : class
        => new(this);

    private static TEntityState? MapState(EntityState? state) => Entities.EntityState.Map<TEntityState>(state);

    public IObservable<StateChange<TEntity, TEntityState>> StateAllChanges() =>
        HaContext.StateAllChanges().Where(e => e.Entity.EntityId == EntityId).Select(e => new StateChange<TEntity, TEntityState>((TEntity)this,
                Entities.EntityState.Map<TEntityState>(e.Old),
                Entities.EntityState.Map<TEntityState>(e.New)));

    public IObservable<StateChange<TEntity, TEntityState>> StateChanges() => StateAllChanges().StateChangesOnly();
}

public record Entity<TAttributes> : Entity<Entity<TAttributes>, EntityState<string ,TAttributes>, TAttributes, string>
    where TAttributes : class
{
    public Entity(IEntityCore entity) : base(entity)
    {
    }

    public Entity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }
}

/// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
public record Entity : Entity<Entity, EntityState<string, object>, object, string>
{
    public Entity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public Entity(IEntityCore entity) : base(entity)
    {
    }
}
