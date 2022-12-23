namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Factory for creating strongly typed generic IEntity instances
/// </summary>
public interface IEntityFactory
{
    /// <summary>
    /// Create a generic IEntity instance where the state is parsed as TState and the attributes are parsed as TAttributes
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TAttributes"></typeparam>
    IEntity<TState, TAttributes> CreateIEntity<TState, TAttributes>(string entityId, IEntityStateMapper<TState, TAttributes> mapper)
        where TAttributes : class;
}