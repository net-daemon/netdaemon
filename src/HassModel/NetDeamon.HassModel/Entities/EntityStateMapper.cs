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
    public IStateChange<TState, TAttributes> Map(IStateChange stateChange) => new StateChangeGeneric<TState, TAttributes>(stateChange, this);

    /// <inheritdoc/>
    public IEntity<TState, TAttributes> Map(IEntity entity) => new EntityGeneric<TState, TAttributes>(entity.HaContext, entity.EntityId, this);

    /// <inheritdoc/>
    public IEntity<TState, TAttributes> Entity(IHaContext haContext, string entityId) => new EntityGeneric<TState, TAttributes>(haContext, entityId, this);

    /// <inheritdoc/>
    public IEntityStateMapper<TStateNew, TAttributes> WithStateAs<TStateNew>(Func<string?, TStateNew> newStateParser)
        => new EntityStateMapper<TStateNew, TAttributes>(newStateParser, _attributesParser);

    /// <inheritdoc/>
    public IEntityStateMapper<TState, TAttributesNew> WithAttributesAs<TAttributesNew>(Func<JsonElement?, TAttributesNew?> customAttributesParser)
        where TAttributesNew : class
        => new EntityStateMapper<TState, TAttributesNew>(_stateParser, customAttributesParser);

    /// <inheritdoc/>
    public IEntityStateMapper<TState, TAttributesNew> WithAttributesAs<TAttributesNew>()
        where TAttributesNew : class
        => WithAttributesAs<TAttributesNew>(DefaultEntityStateMappers.AttributesAsClass<TAttributesNew>);
}


/// <inheritdoc/>
public sealed class CachedEntityStateMapper<TState, TAttributes> : IEntityStateMapper<TState, TAttributes>
    where TAttributes : class
{
    private readonly Func<string?, TState> _stateParser;
    private readonly Func<JsonElement?, TAttributes?> _attributesParser;

    /// <summary>
    /// Create an EnitityStateMapper by providing parsinge functions for the state and attributes properties
    /// </summary>
    /// <param name="stateParser"></param>
    /// <param name="attributesParser"></param>
    public CachedEntityStateMapper(Func<string?, TState> stateParser, Func<JsonElement?, TAttributes?> attributesParser)
    {
        _stateParser = stateParser;
        _attributesParser = attributesParser;
    }

    /// <inheritdoc/>
    public TState ParseState(string? rawState) => _stateParser(rawState);

    /// <inheritdoc/>
    public TAttributes? ParseAttributes(JsonElement? rawAttributes) => _attributesParser(rawAttributes);

    /// <inheritdoc/>
    public IEntityState<TState, TAttributes> Map(IEntityState state) => new CachedEntityStateGeneric<TState, TAttributes>(state, this);

    /// <inheritdoc/>
    public IStateChange<TState, TAttributes> Map(IStateChange stateChange) => new StateChangeGeneric<TState, TAttributes>(stateChange, this);

    /// <inheritdoc/>
    public IEntity<TState, TAttributes> Map(IEntity entity) => new EntityGeneric<TState, TAttributes>(entity.HaContext, entity.EntityId, this);

    /// <inheritdoc/>
    public IEntity<TState, TAttributes> Entity(IHaContext haContext, string entityId) => new EntityGeneric<TState, TAttributes>(haContext, entityId, this);

    /// <inheritdoc/>
    public IEntityStateMapper<TStateNew, TAttributes> WithStateAs<TStateNew>(Func<string?, TStateNew> newStateParser)
        => new CachedEntityStateMapper<TStateNew, TAttributes>(newStateParser, _attributesParser);

    /// <inheritdoc/>
    public IEntityStateMapper<TState, TAttributesNew> WithAttributesAs<TAttributesNew>(Func<JsonElement?, TAttributesNew?> customAttributesParser)
        where TAttributesNew : class
        => new CachedEntityStateMapper<TState, TAttributesNew>(_stateParser, customAttributesParser);

    /// <inheritdoc/>
    public IEntityStateMapper<TState, TAttributesNew> WithAttributesAs<TAttributesNew>()
        where TAttributesNew : class
        => WithAttributesAs<TAttributesNew>(DefaultEntityStateMappers.AttributesAsClass<TAttributesNew>);
}