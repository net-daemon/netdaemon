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
    /// Calls a service using this entity as the target
    /// </summary>
    /// <param name="service">Name of the service to call. If the Domain of the service is the same as the domain of the Entity it can be omitted</param>
    /// <param name="data">Data to provide</param>
    public virtual void CallService(string service, object? data = null)
    {
        EntityExtensions.CallService(this, service, data);
    }

    /// <summary>Gets a new Entity from this Entity with the specified type of attributes</summary>
    public virtual Entity<EntityState<string ,TAttributes>, TAttributes, string> WithAttributesAs<TAttributes>()
        where TAttributes : class
        => new(this);
}

/// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
public record Entity<TEntityState, TAttributes, TState> : Entity
    where TEntityState : EntityState<TState ,TAttributes>
    where TAttributes : class
{
    public new TState? State => EntityState.State;

    /// <summary>Copy constructor from IEntityCore</summary>
    public Entity(IEntityCore entity) : base(entity)
    { }

    /// <summary>Constructor from haContext and entityId</summary>
    public Entity(IHaContext haContext, string entityId) : base(haContext, entityId)
    { }

    /// <inheritdoc />
    public override TAttributes? Attributes => EntityState?.Attributes;

    /// <inheritdoc />
    public override TEntityState? EntityState => MapState(base.EntityState);

    /// <summary>Gets a NumericEntity from a given Entity</summary>
    public Entity<EntityState<double? ,TAttributes>, TAttributes, double?> AsNumeric() => new(this);

    /// <summary>Gets a new Entity from this Entity with the specified type of attributes</summary>
    public override Entity<EntityState<string ,T>, T, string> WithAttributesAs<T>()
        where T : class
        => new(this);

    private static TEntityState? MapState(EntityState? state) => Entities.EntityState.Map<TEntityState>(state);
}

public interface IEntity<out TEntity, out TEntityState, out TAttributes, out TState> : IEntityCore
{

}
