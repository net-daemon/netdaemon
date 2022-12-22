namespace NetDaemon.HassModel.Entities;

public interface IStateChange
{
    IEntity Entity { get; }

    IEntityState? RawOld { get; }

    IEntityState? RawNew { get; }
}

public interface IStateChange<TState, TAttributes> : IStateChange
    where TAttributes : class
{
    IEntityState<TState, TAttributes>? Old { get; }

    IEntityState<TState, TAttributes>? New { get; }
}