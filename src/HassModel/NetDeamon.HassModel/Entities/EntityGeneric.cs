namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public sealed record EntityGeneric<TState, TAttributes> : IEntity<TState, TAttributes>
    where TAttributes : class
{
    /// <inheritdoc/>
    public IHaContext HaContext { get; }

    /// <inheritdoc/>
    public string EntityId { get; }

    /// <inheritdoc/>
    public IEntityStateMapper<TState, TAttributes> EntityStateMapper { get; }

    /// <inheritdoc/>
    public IEntityState? RawEntityState => HaContext.GetStateGeneric(EntityId);

    /// <inheritdoc/>
    public string? RawState => RawEntityState?.RawState;

    internal EntityGeneric(IHaContext haContext, string entityId, IEntityStateMapper<TState, TAttributes> mapper)
    {
        HaContext = haContext;
        EntityId = entityId;
        EntityStateMapper = mapper;
    }

    /// <inheritdoc/>
    public IEntityState<TState, TAttributes>? EntityState =>
        RawEntityState is null
            ? null
            : EntityStateMapper.Map(RawEntityState);

    /// <inheritdoc/>
    public TState State => EntityState is null ? EntityStateMapper.ParseState(null) : EntityState.State;

    /// <inheritdoc/>
    public TAttributes? Attributes => EntityState is null ? EntityStateMapper.ParseAttributes(null) : EntityState.Attributes;

    /// <inheritdoc/>
    public IObservable<IStateChange> RawStateAllChanges() =>
        HaContext.StateAllChangesGeneric().Where(e => e.Entity.EntityId == EntityId);

    /// <inheritdoc/>
    public IObservable<IStateChange> RawStateChanges() =>
        RawStateAllChanges().StateChangesOnlyGeneric();

    /// <inheritdoc/>
    public IObservable<IStateChange<TState, TAttributes>> StateAllChanges() =>
        RawStateAllChanges().Select(EntityStateMapper.Map);

    /// <inheritdoc/>
    public IObservable<IStateChange<TState, TAttributes>> StateChanges() =>
        StateAllChanges().StateChangesOnlyGeneric();

    /// <inheritdoc/>
    public IEntity<TStateNew, TAttributes> WithStateAs<TStateNew>(Func<string?, TStateNew> newStateParser)
        => EntityStateMapper.WithStateAs(newStateParser).Map(this);

    /// <inheritdoc/>
    public IEntity<TState, TAttributesNew> WithAttributesAs<TAttributesNew>(Func<JsonElement?, TAttributesNew> newAttributesParser)
        where TAttributesNew : class
        => EntityStateMapper.WithAttributesAs(newAttributesParser).Map(this);

    /// <inheritdoc/>
    public IEntity<TState, TAttributesNew> WithAttributesAs<TAttributesNew>()
        where TAttributesNew : class
        => EntityStateMapper.WithAttributesAs<TAttributesNew>().Map(this);

    /// <inheritdoc/>
    public void CallService(string service, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));

        var (serviceDomain, serviceName) = service.SplitAtDot();

        serviceDomain ??= EntityId.SplitAtDot().Left ?? throw new InvalidOperationException("EntityId must be formatted 'domain.name'");

        HaContext.CallService(serviceDomain, serviceName, ServiceTarget.FromEntity(EntityId), data);
    }
}