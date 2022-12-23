namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Represents a state change event for an entity
/// </summary>
public interface IStateChange
{
    /// <summary>
    /// The entity associated with the state change
    /// </summary>
    /// <value></value>
    IEntity Entity { get; }

    /// <summary>
    /// The old raw state of the entity
    /// </summary>
    /// <value></value>
    IEntityState? RawOld { get; }

    /// <summary>
    /// The new raw state of the entity
    /// </summary>
    /// <value></value>
    IEntityState? RawNew { get; }
}

/// <summary>
/// Represents a state change event for a strongly typed entity and state
/// </summary>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TAttributes"></typeparam>
public interface IStateChange<TState, TAttributes> : IStateChange
    where TAttributes : class
{
    /// <summary>
    /// The strongly typed old state
    /// </summary>
    /// <value></value>
    IEntityState<TState, TAttributes>? Old { get; }

    /// <summary>
    /// The strongly typed new state
    /// </summary>
    /// <value></value>
    IEntityState<TState, TAttributes>? New { get; }
}