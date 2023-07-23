namespace NetDaemon.HassModel.Entities;

/// <summary>
/// 
/// </summary>
public interface IEntityTarget: IEntity
{
    /// <summary>
    /// Base interface for all service targets such as entities, rooms, areas.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="data"></param>
    public void CallService(string service, object? data = null);
}
