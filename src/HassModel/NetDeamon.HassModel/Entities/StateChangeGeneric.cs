namespace NetDaemon.HassModel.Entities;

public record StateChangeGeneric : IStateChange
{
    public required IEntity Entity { get; init; }

    public IEntityState? RawOld { get; init; }

    public IEntityState? RawNew { get; init; }
}

public record StateChangeGeneric<TState, TAttributes> : IStateChange<TState, TAttributes>
    where TAttributes : class
{
    private readonly IStateChange _stateChange;
    private readonly IEntityStateMapper<TState, TAttributes> _mapper;

    public StateChangeGeneric(IStateChange stateChange, IEntityStateMapper<TState, TAttributes> mapper)
    {
        _stateChange = stateChange;
        _mapper = mapper;
    }

    public IEntityState<TState, TAttributes>? Old =>
        _stateChange.RawOld is null
            ? null
            : _mapper.Map(_stateChange.RawOld);

    public IEntityState<TState, TAttributes>? New =>
        _stateChange.RawNew is null
            ? null
            : _mapper.Map(_stateChange.RawNew);

    public IEntity Entity => _stateChange.Entity;

    public IEntityState? RawOld => _stateChange.RawOld;

    public IEntityState? RawNew => _stateChange.RawNew;
}