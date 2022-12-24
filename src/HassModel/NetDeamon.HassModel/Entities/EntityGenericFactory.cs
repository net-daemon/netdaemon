namespace NetDaemon.HassModel.Entities;

/// <inheritdoc/>
public sealed class EntityGenericFactory : IEntityFactory
{
    private readonly IHaContext _haContext;

    /// <summary>
    /// Create an IEntityFactory that produces IEntity instances for the provided Home Assistant Context
    /// </summary>
    /// <param name="haContext">Home Assistant Context used with produced entities</param>
    public EntityGenericFactory(IHaContext haContext) => _haContext = haContext;

    /// <inheritdoc/>
    public IEntity<TState, TAttributes> CreateIEntity<TState, TAttributes>(string entityId, IEntityStateMapper<TState, TAttributes> mapper)
        where TAttributes : class
        => mapper.Entity(_haContext, entityId);
}