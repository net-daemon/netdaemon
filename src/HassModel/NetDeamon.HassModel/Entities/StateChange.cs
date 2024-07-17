namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Represents a state change event for an entity
/// </summary>
public record StateChange
{
    private readonly JsonElement _jsonElement;
    private readonly IHaContext _haContext;
    private Entity? _entity;
    private EntityState? _old;
    private EntityState? _new;

    /// <summary>
    /// Creates a StateChange from a jsonElement and lazy load the states
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="haContext"></param>
    internal StateChange(JsonElement jsonElement, IHaContext haContext)
    {
        _jsonElement = jsonElement;
        _haContext = haContext;
    }

    /// <summary>
    /// This should not be used under normal circumstances but can be used for unit testing of apps
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="old"></param>
    /// <param name="new"></param>
    public StateChange(Entity entity, EntityState? old, EntityState? @new)
    {
        _entity = entity;
        _new    = @new;
        _old    = old;
        _haContext = null!; // haContext is not used when _entity is already initialized
    }

    /// <summary>The Entity that changed</summary>
    public virtual Entity Entity => _entity ??= new Entity(_haContext, _jsonElement.GetProperty("entity_id").GetString() ?? throw  new InvalidOperationException("No Entity_id in state_change event"));

    /// <summary>The old state of the entity</summary>
    public virtual EntityState? Old => _old ??= _jsonElement.GetProperty("old_state").Deserialize<HassState>().Map();

    /// <summary>The new state of the entity</summary>
    public virtual EntityState? New => _new ??= _jsonElement.GetProperty("new_state").Deserialize<HassState>().Map();
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
