namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Core  interface for any entity
/// </summary>
public interface IEntityCore
{
    /// <summary>
    /// The IHAContext
    /// </summary>
    public IHaContext HaContext { get; }

    /// <summary>
    /// Entity id being handled by this entity
    /// </summary>
    public string EntityId { get; }
}