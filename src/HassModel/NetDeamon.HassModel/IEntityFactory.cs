namespace NetDaemon.HassModel;

/// <summary>
/// Interface for creating Entities based on a HaContext and an EntityId
/// </summary>
/// <remarks>
/// The Code Generator will generate a class that implements this interface to create entities of the apropriate generated types
/// </remarks>
public interface IEntityFactory
{
    /// <summary>
    /// Creates a (derived) Entity from a haContext and EntityId. To be implemented by the code generator
    /// </summary>
    /// <param name="haContext"></param>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public Entity CreateEntity(IHaContext haContext, string entityId);
}
