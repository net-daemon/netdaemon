namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public sealed class EntityStateMapper<TState, TAttributes> : IEntityStateMapper<TState, TAttributes>
    where TAttributes : class
{
    private readonly Func<string?, TState> _stateParser;
    private readonly Func<JsonElement?, TAttributes?> _attributesParser;

    /// <summary>
    /// Create an EnitityStateMapper by providing parsinge functions for the state and attributes properties
    /// </summary>
    /// <param name="stateParser"></param>
    /// <param name="attributesParser"></param>
    public EntityStateMapper(Func<string?, TState> stateParser, Func<JsonElement?, TAttributes?> attributesParser)
    {
        _stateParser = stateParser;
        _attributesParser = attributesParser;
    }

    /// <inheritdoc/>
    public TState ParseState(string? rawState) => _stateParser(rawState);

    /// <inheritdoc/>
    public TAttributes? ParseAttributes(JsonElement? rawAttributes) => _attributesParser(rawAttributes);

    /// <inheritdoc/>
    public IEntityState<TState, TAttributes> Map(IEntityState state) => new EntityStateGeneric<TState, TAttributes>(state, this);

    /// <inheritdoc/>
    public IEntity<TState, TAttributes> Map(IEntity entity) => new EntityGeneric<TState, TAttributes>(entity, this);

    /// <inheritdoc/>
    public IStateChange<TState, TAttributes> Map(IStateChange stateChange) => new StateChangeGeneric<TState, TAttributes>(stateChange, this);
}