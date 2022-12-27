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

    internal EntityGeneric(IHaContext haContext, string entityId, IEntityStateMapper<TState, TAttributes> mapper)
    {
        HaContext = haContext;
        EntityId = entityId;
        EntityStateMapper = mapper;
    }

    /// <inheritdoc/>
    public IEntityState<TState, TAttributes>? EntityState => EntityStateMapper.MapHassState(HaContext.GetHassState(EntityId));

    /// <inheritdoc/>
    public TState State => EntityState is null ? EntityStateMapper.ParseState(null) : EntityState.State;

    /// <inheritdoc/>
    public TAttributes? Attributes => EntityState is null ? EntityStateMapper.ParseAttributes(null) : EntityState.Attributes;

    /// <inheritdoc/>
    public IObservable<IStateChange<TState, TAttributes>> StateAllChanges() =>
        HaContext.HassStateAllChanges().Select(e => EntityStateMapper.MapHassStateChange(HaContext, e)).Where(e => e.Entity.EntityId == EntityId);

    /// <inheritdoc/>
    public IObservable<IStateChange<TState, TAttributes>> StateChanges() =>
        StateAllChanges().Where(c => c.New?.RawState != c.Old?.RawState);

    /// <inheritdoc/>
    public void CallService(string service, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(service, nameof(service));

        var (serviceDomain, serviceName) = service.SplitAtDot();

        serviceDomain ??= EntityId.SplitAtDot().Left ?? throw new InvalidOperationException("EntityId must be formatted 'domain.name'");

        HaContext.CallService(serviceDomain, serviceName, ServiceTarget.FromEntity(EntityId), data);
    }
}