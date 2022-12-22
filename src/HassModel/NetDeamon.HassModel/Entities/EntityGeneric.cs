namespace NetDaemon.HassModel.Entities;

public record EntityGeneric : IEntity
{
    public IHaContext HaContext { get; }

    public string EntityId { get; }

    public EntityGeneric(IHaContext haContext, string entityId)
    {
        HaContext = haContext;
        EntityId = entityId;
    }

    public IEntityState? RawEntityState => HaContext.GetStateGeneric(EntityId);

    public string? RawState => RawEntityState?.RawState;

    public IObservable<IStateChange> RawStateAllChanges() =>
        HaContext.StateAllChangesGeneric().Where(e => e.Entity.EntityId == EntityId);

    public IObservable<IStateChange> RawStateChanges() =>
        RawStateAllChanges().StateChangesOnlyGeneric();

    public void CallService(string service, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));

        var (serviceDomain, serviceName) = service.SplitAtDot();

        serviceDomain ??= EntityId.SplitAtDot().Left ?? throw new InvalidOperationException("EntityId must be formatted 'domain.name'");

        HaContext.CallService(serviceDomain, serviceName, ServiceTarget.FromEntity(EntityId), data);
    }
}

public record EntityGeneric<TState, TAttributes> : IEntity<TState, TAttributes>
    where TAttributes : class
{
    private readonly IEntity _entity;
    private readonly IEntityStateMapper<TState, TAttributes> _mapper;

    public EntityGeneric(IEntity entity, Func<string?, TState> stateParser, Func<JsonElement?, TAttributes> attributesParser)
    {
        _entity = entity;
        _mapper = new EntityStateMapper<TState, TAttributes>(stateParser, attributesParser);
    }

    public EntityGeneric(IEntity entity, IEntityStateMapper<TState, TAttributes> mapper)
    {
        _entity = entity;
        _mapper = mapper;
    }

    public IHaContext HaContext => _entity.HaContext;

    public string EntityId => _entity.EntityId;

    public IEntityState? RawEntityState => _entity.RawEntityState;

    public IEntityState<TState, TAttributes>? EntityState =>
        _entity.RawEntityState is null
            ? null
            : _mapper.Map(_entity.RawEntityState);
    public TState State => EntityState is null ? _mapper.ParseState(null) : EntityState.State;

    public string? RawState => _entity.RawState;

    public TAttributes Attributes => EntityState is null ? _mapper.ParseAttributes(null) : EntityState.Attributes;

    public void CallService(string service, object? data = null) => _entity.CallService(service, data);

    public IObservable<IStateChange> RawStateAllChanges() => _entity.RawStateAllChanges();

    public IObservable<IStateChange> RawStateChanges() => _entity.RawStateChanges();

    public IObservable<IStateChange<TState, TAttributes>> StateAllChanges() =>
        _entity.RawStateAllChanges().Select(_mapper.Map);

    public IObservable<IStateChange<TState, TAttributes>> StateChanges() =>
        StateAllChanges().StateChangesOnlyGeneric();
}