namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public sealed record EntityStateGeneric
(
    string EntityId,
    string? RawState = null,
    JsonElement? AttributesJson = null,
    DateTime? LastChanged = null,
    DateTime? LastUpdated = null,
    Context? Context = null
) : IEntityState;

/// <summary>
/// Detailed state information where a strongly typed value is parsed each time it's accessed.
/// This is the default behavior and ideal when the type is easily (or even trivially parsed if
/// it's a string or wrapper of a string).
/// </summary>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TAttributes"></typeparam>
public sealed record EntityStateGeneric<TState, TAttributes> : IEntityState<TState, TAttributes>
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

/// <summary>
/// Detailed state information stored as a cached strongly typed value.
/// The parsed value is cached.
/// </summary>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TAttributes"></typeparam>
public sealed record CachedEntityStateGeneric<TState, TAttributes> : IEntityState<TState, TAttributes>
    where TAttributes : class
{
    private readonly IEntityState _entityState;
    private readonly Lazy<TState> _stateLazy;
    private readonly Lazy<TAttributes?> _attributesLazy;

    internal CachedEntityStateGeneric(IEntityState entityState, IEntityStateMapper<TState, TAttributes> mapper)
    {
        _entityState = entityState;
        _stateLazy = new (() => mapper.ParseState(_entityState.RawState));
        _attributesLazy = new (() => mapper.ParseAttributes(_entityState.AttributesJson));
    }

    /// <inheritdoc/>
    public TState State => _stateLazy.Value;

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