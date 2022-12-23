namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public sealed record EntityGeneric : IEntity
{
    /// <inheritdoc/>
    public IHaContext HaContext { get; }

    /// <inheritdoc/>
    public string EntityId { get; }

    internal EntityGeneric(IHaContext haContext, string entityId)
    {
        HaContext = haContext;
        EntityId = entityId;
    }

    /// <inheritdoc/>
    public IEntityState? RawEntityState => HaContext.GetStateGeneric(EntityId);

    /// <inheritdoc/>
    public string? RawState => RawEntityState?.RawState;

    /// <inheritdoc/>
    public IObservable<IStateChange> RawStateAllChanges() =>
        HaContext.StateAllChangesGeneric().Where(e => e.Entity.EntityId == EntityId);

    /// <inheritdoc/>
    public IObservable<IStateChange> RawStateChanges() =>
        RawStateAllChanges().StateChangesOnlyGeneric();

    /// <inheritdoc/>
    public void CallService(string service, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));

        var (serviceDomain, serviceName) = service.SplitAtDot();

        serviceDomain ??= EntityId.SplitAtDot().Left ?? throw new InvalidOperationException("EntityId must be formatted 'domain.name'");

        HaContext.CallService(serviceDomain, serviceName, ServiceTarget.FromEntity(EntityId), data);
    }
}

/// <inheritdoc/>
public sealed record EntityGeneric<TState, TAttributes> : IEntity<TState, TAttributes>
    where TAttributes : class
{
    private readonly IEntity _entity;

    internal EntityGeneric(IEntity entity, IEntityStateMapper<TState, TAttributes> mapper)
    {
        _entity = entity;
        EntityStateMapper = mapper;
    }

    /// <inheritdoc/>
    public IEntityStateMapper<TState, TAttributes> EntityStateMapper { get; }

    /// <inheritdoc/>
    public IHaContext HaContext => _entity.HaContext;

    /// <inheritdoc/>
    public string EntityId => _entity.EntityId;

    /// <inheritdoc/>
    public IEntityState? RawEntityState => _entity.RawEntityState;

    /// <inheritdoc/>
    public IEntityState<TState, TAttributes>? EntityState =>
        _entity.RawEntityState is null
            ? null
            : EntityStateMapper.Map(_entity.RawEntityState);

    /// <inheritdoc/>
    public TState State => EntityState is null ? EntityStateMapper.ParseState(null) : EntityState.State;

    /// <inheritdoc/>
    public string? RawState => _entity.RawState;

    /// <inheritdoc/>
    public TAttributes? Attributes => EntityState is null ? EntityStateMapper.ParseAttributes(null) : EntityState.Attributes;

    /// <inheritdoc/>
    public void CallService(string service, object? data = null) => _entity.CallService(service, data);

    /// <inheritdoc/>
    public IObservable<IStateChange> RawStateAllChanges() => _entity.RawStateAllChanges();

    /// <inheritdoc/>
    public IObservable<IStateChange> RawStateChanges() => _entity.RawStateChanges();

    /// <inheritdoc/>
    public IObservable<IStateChange<TState, TAttributes>> StateAllChanges() =>
        _entity.RawStateAllChanges().Select(EntityStateMapper.Map);

    /// <inheritdoc/>
    public IObservable<IStateChange<TState, TAttributes>> StateChanges() =>
        StateAllChanges().StateChangesOnlyGeneric();

    /// <inheritdoc/>
    IEntity<TState, TAttributesNew> IEntity<TState, TAttributes>.WithAttributesAs<TAttributesNew>(Func<JsonElement?, TAttributesNew> newAttributesParser)
        where TAttributesNew : class
        => EntityStateMapper.WithAttributesAs(newAttributesParser).Map(this);

    /// <inheritdoc/>
    IEntity<TState, TAttributesNew> IEntity<TState, TAttributes>.WithAttributesAs<TAttributesNew>()
        where TAttributesNew : class
        => EntityStateMapper.WithAttributesAs<TAttributesNew>().Map(this);

    /// <inheritdoc/>
    IEntity<TStateNew, TAttributes> IEntity<TState, TAttributes>.WithStateAs<TStateNew>(Func<string?, TStateNew> newStateParser)
        => EntityStateMapper.WithStateAs(newStateParser).Map(this);
}