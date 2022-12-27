namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Represents a state change event for a strongly typed entity and state
/// </summary>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TAttributes"></typeparam>
public interface IStateChange<TState, TAttributes>
    where TAttributes : class
{
    /// <summary>
    /// The strongly typed entity object
    /// </summary>
    /// <value></value>
    IEntity<TState, TAttributes> Entity { get; }

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