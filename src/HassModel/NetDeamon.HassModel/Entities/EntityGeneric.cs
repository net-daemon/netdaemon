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
    private readonly IEntityStateMapper<TState, TAttributes> _mapper;

    internal EntityGeneric(IEntity entity, IEntityStateMapper<TState, TAttributes> mapper)
    {
        _entity = entity;
        _mapper = mapper;
    }

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
            : _mapper.Map(_entity.RawEntityState);

    /// <inheritdoc/>
    public TState State => EntityState is null ? _mapper.ParseState(null) : EntityState.State;

    /// <inheritdoc/>
    public string? RawState => _entity.RawState;

    /// <inheritdoc/>
    public TAttributes? Attributes => EntityState is null ? _mapper.ParseAttributes(null) : EntityState.Attributes;

    /// <inheritdoc/>
    public void CallService(string service, object? data = null) => _entity.CallService(service, data);

    /// <inheritdoc/>
    public IObservable<IStateChange> RawStateAllChanges() => _entity.RawStateAllChanges();

    /// <inheritdoc/>
    public IObservable<IStateChange> RawStateChanges() => _entity.RawStateChanges();

    /// <inheritdoc/>
    public IObservable<IStateChange<TState, TAttributes>> StateAllChanges() =>
        _entity.RawStateAllChanges().Select(_mapper.Map);

    /// <inheritdoc/>
    public IObservable<IStateChange<TState, TAttributes>> StateChanges() =>
        StateAllChanges().StateChangesOnlyGeneric();
}