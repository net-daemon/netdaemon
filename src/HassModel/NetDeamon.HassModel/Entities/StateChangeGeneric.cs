namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public sealed record StateChangeGeneric<TState, TAttributes>
(
    IEntity<TState, TAttributes> Entity,
    IEntityState<TState, TAttributes>? Old,
    IEntityState<TState, TAttributes>? New
) : IStateChange<TState, TAttributes>
    where TAttributes : class;