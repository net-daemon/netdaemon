namespace NetDaemon.HassModel.Entities;

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
    /// <inheritdoc/>
    public string EntityId { get; }

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

    private readonly Func<string?, TState> _parseState;
    private readonly Lazy<TAttributes?> _attributesLazy;

    internal EntityStateGeneric(string entityId, IEntityStateMapper<TState, TAttributes> mapper)
    {
        EntityId = entityId;
        _parseState = mapper.ParseState;
        _attributesLazy = new (() => mapper.ParseAttributes(AttributesJson));
    }

    /// <inheritdoc/>
    public TState State => _parseState(RawState);

    /// <inheritdoc/>
    public TAttributes? Attributes => _attributesLazy.Value;
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
    /// <inheritdoc/>
    public string EntityId { get; }

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

    private readonly Lazy<TState> _stateLazy;
    private readonly Lazy<TAttributes?> _attributesLazy;

    internal CachedEntityStateGeneric(string entityId, IEntityStateMapper<TState, TAttributes> mapper)
    {
        EntityId = entityId;
        _stateLazy = new (() => mapper.ParseState(RawState));
        _attributesLazy = new (() => mapper.ParseAttributes(AttributesJson));
    }

    /// <inheritdoc/>
    public TState State => _stateLazy.Value;

    /// <inheritdoc/>
    public TAttributes? Attributes => _attributesLazy.Value;
}