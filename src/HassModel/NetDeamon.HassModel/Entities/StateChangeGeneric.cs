namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public sealed record StateChangeGeneric
(
    IEntity Entity,
    IEntityState? RawOld,
    IEntityState? RawNew
) : IStateChange;

/// <inheritdoc/>
public sealed record StateChangeGeneric<TState, TAttributes> : IStateChange<TState, TAttributes>
    where TAttributes : class
{
    private readonly IStateChange _stateChange;
    private readonly IEntityStateMapper<TState, TAttributes> _mapper;

    internal StateChangeGeneric(IStateChange stateChange, IEntityStateMapper<TState, TAttributes> mapper)
    {
        _stateChange = stateChange;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public IEntityState<TState, TAttributes>? Old =>
        _stateChange.RawOld is null
            ? null
            : _mapper.Map(_stateChange.RawOld);

    /// <inheritdoc/>
    public IEntityState<TState, TAttributes>? New =>
        _stateChange.RawNew is null
            ? null
            : _mapper.Map(_stateChange.RawNew);

    /// <inheritdoc/>
    public IEntity Entity => _stateChange.Entity;

    /// <inheritdoc/>
    public IEntityState? RawOld => _stateChange.RawOld;

    /// <inheritdoc/>
    public IEntityState? RawNew => _stateChange.RawNew;
}