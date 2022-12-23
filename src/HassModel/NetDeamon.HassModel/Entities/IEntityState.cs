namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Detailec state information
/// </summary>
public interface IEntityState
{
    /// <summary>
    /// Unique id of the entity
    /// </summary>
    /// <value></value>
    string EntityId { get; }

    /// <summary>
    /// The raw state of the entity as the original nullable string
    /// </summary>
    /// <value></value>
    string? RawState { get; }

    /// <summary>
    /// The raw attributes as the original JSON
    /// </summary>
    /// <value></value>
    JsonElement? AttributesJson { get; }

    /// <summary>
    /// When the state or attributes last changed
    /// </summary>
    /// <value></value>
    DateTime? LastChanged { get; }

    /// <summary>
    /// When the state or attributes were last update (even if they didn't change)
    /// </summary>
    /// <value></value>
    DateTime? LastUpdated { get; }

    /// <summary>
    /// Home Assistant Context
    /// </summary>
    /// <value></value>
    Context? Context { get; }
}

/// <summary>
/// Generic EntityState with specific types of State and Attributes
/// </summary>
/// <typeparam name="TState">Type of the State property</typeparam>
/// <typeparam name="TAttributes">Type of the Attributes property</typeparam>
public interface IEntityState<TState, TAttributes> : IEntityState
    where TAttributes : class
{
    /// <summary>
    /// The state of the entity as the type TState
    /// </summary>
    /// <value></value>
    TState State { get; }

    /// <summary>
    /// The attributes of the entity as the class TAttributes (possibly null)
    /// </summary>
    /// <value></value>
    TAttributes? Attributes { get; }
}