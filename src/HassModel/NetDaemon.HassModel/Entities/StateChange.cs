namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Represents a state change event for an entity
/// </summary>
public record StateChange
{
    private readonly Lazy<Entity> _entity;
    private readonly Lazy<EntityState?> _old;
    private readonly Lazy<EntityState?> _new;

    /// <summary>
    /// Creates a StateChange from a jsonElement and lazy load the states
    /// </summary>
    internal StateChange(JsonElement jsonElement, IHaContext haContext)
    {
        _entity = new Lazy<Entity>(() =>
            haContext.Entity(jsonElement.GetProperty("entity_id").GetString() ?? throw new InvalidOperationException("No Entity_id in state_change event")));
        _new = new Lazy<EntityState?>(() => jsonElement.GetProperty("new_state").Deserialize<EntityState>());
        _old = new Lazy<EntityState?>(() => jsonElement.GetProperty("old_state").Deserialize<EntityState>());
    }

    /// <summary>
    /// This should not be used under normal circumstances but can be used for unit testing of apps
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="old"></param>
    /// <param name="new"></param>
    public StateChange(Entity entity, EntityState? old, EntityState? @new)
    {
        _entity = new Lazy<Entity>(() => entity);
        _new = new Lazy<EntityState?>(() => @new);
        _old = new Lazy<EntityState?>(() => old);
    }

    /// <summary>The Entity that changed</summary>
    public virtual Entity Entity => _entity.Value;

    /// <summary>The old state of the entity</summary>
    public virtual EntityState? Old => _old.Value;

    /// <summary>The new state of the entity</summary>
    public virtual EntityState? New => _new.Value;
}

/// <summary>
/// Represents a state change event for a strong typed entity and state
/// </summary>
/// <typeparam name="TEntity">The Type</typeparam>
/// <typeparam name="TEntityState"></typeparam>
public record StateChange<TEntity, TEntityState> : StateChange
    where TEntity : Entity
    where TEntityState : EntityState
{
    /// <summary>
    /// This should not be used under normal circumstances but can be used for unit testing of apps
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="old"></param>
    /// <param name="new"></param>
    public StateChange(TEntity entity, TEntityState? old, TEntityState? @new) : base(entity, old, @new)
    {
    }

    /// <inheritdoc/>
    public override TEntity Entity => (TEntity)base.Entity;

    /// <inheritdoc/>
    public override TEntityState? New => (TEntityState?)base.New;

    /// <inheritdoc/>
    public override TEntityState? Old => (TEntityState?)base.Old;
}
