namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Base interface for all entities in NetDaemon
/// </summary>
public interface IEntity
{
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