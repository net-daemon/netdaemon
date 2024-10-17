namespace NetDaemon.HassModel;

/// <summary>
/// Provides a default implementation of IEntityFactory in case no generated factory is registered
/// </summary>
internal class DefaultEntityFactory : IEntityFactory
{
    public Entity CreateEntity(IHaContext haContext, string entityId) => new(haContext, entityId);
}
