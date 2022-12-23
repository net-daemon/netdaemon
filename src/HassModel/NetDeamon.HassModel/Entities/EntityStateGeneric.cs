namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public class EntityStateGeneric : IEntityState
{
    /// <inheritdoc/>
    public required string EntityId { get; init; }

    /// <inheritdoc/>
    public string? RawState { get; init; }

    /// <inheritdoc/>
    public JsonElement? AttributesJson { get; init; }

    /// <inheritdoc/>
    public DateTime? LastChanged { get; init; }

    /// <inheritdoc/>
    public DateTime? LastUpdated { get; init; }

    /// <inheritdoc/>
    public Context? Context { get; init; }
}

/// <inheritdoc/>
public class EntityStateGeneric<TState, TAttributes> : IEntityState<TState, TAttributes>
    where TAttributes : class
{
    private readonly IEntityState _entityState;
    private readonly Lazy<TAttributes?> _attributesLazy;
    private readonly IEntityStateMapper<TState, TAttributes> _mapper;

    internal EntityStateGeneric(IEntityState entityState, IEntityStateMapper<TState, TAttributes> mapper)
    {
        _entityState = entityState;
        _mapper = mapper;
        _attributesLazy = new (() => _mapper.ParseAttributes(_entityState.AttributesJson));
    }

    /// <inheritdoc/>
    public TState State => _mapper.ParseState(_entityState.RawState);

    /// <inheritdoc/>
    public TAttributes? Attributes => _attributesLazy.Value;

    /// <inheritdoc/>
    public string EntityId => _entityState.EntityId;

    /// <inheritdoc/>
    public string? RawState => _entityState.RawState;

    /// <inheritdoc/>
    public JsonElement? AttributesJson => _entityState.AttributesJson;

    /// <inheritdoc/>
    public DateTime? LastChanged => _entityState.LastChanged;

    /// <inheritdoc/>
    public DateTime? LastUpdated => _entityState.LastUpdated;

    /// <inheritdoc/>
    public Context? Context => _entityState.Context;
}