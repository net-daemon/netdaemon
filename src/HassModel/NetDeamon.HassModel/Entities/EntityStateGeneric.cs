namespace NetDaemon.HassModel.Entities;

public class EntityStateGeneric : IEntityState
{
    public required string EntityId { get; init; }
    public string? RawState { get; init; }
    public JsonElement? AttributesJson { get; init; }
    public DateTime? LastChanged { get; init; }
    public DateTime? LastUpdated { get; init; }
    public Context? Context { get; init; }
}

public class EntityStateGeneric<TState, TAttributes> : IEntityState<TState, TAttributes>
    where TAttributes : class
{
    private readonly IEntityState _entityState;
    private readonly Lazy<TAttributes> _attributesLazy;
    private readonly IEntityStateMapper<TState, TAttributes> _mapper;

    public EntityStateGeneric(IEntityState entityState, IEntityStateMapper<TState, TAttributes> mapper)
    {
        _entityState = entityState;
        _mapper = mapper;
        _attributesLazy = new (() => _mapper.ParseAttributes(_entityState.AttributesJson));
    }

    public TState State => _mapper.ParseState(_entityState.RawState);

    public TAttributes Attributes => _attributesLazy.Value;

    public string EntityId => _entityState.EntityId;
    public string? RawState => _entityState.RawState;
    public JsonElement? AttributesJson => _entityState.AttributesJson;
    public DateTime? LastChanged => _entityState.LastChanged;
    public DateTime? LastUpdated => _entityState.LastUpdated;
    public Context? Context => _entityState.Context;
}