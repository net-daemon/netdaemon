namespace NetDaemon.HassModel.Entities;

/// <summary>
/// 
/// </summary>
public interface IEntityTarget
{
    /// <summary>
    /// Base interface for all service targets such as entities, rooms, areas.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="data"></param>
    public void CallService(string service, object? data = null);
    /// <summary>
    /// 
    /// </summary>
    public string EntityId { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public IHaContext? HaContext { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IObservable<StateChange> StateAllChanges();
}
