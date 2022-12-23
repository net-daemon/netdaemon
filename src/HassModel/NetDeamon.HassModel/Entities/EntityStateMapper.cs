namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public class EntityStateMapper<TState, TAttributes> : IEntityStateMapper<TState, TAttributes>
    where TAttributes : class
{
    private readonly Func<string?, TState> _stateParser;
    private readonly Func<JsonElement?, TAttributes?> _attributesParser;

    internal EntityStateMapper(Func<string?, TState> stateParser, Func<JsonElement?, TAttributes?> attributesParser)
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