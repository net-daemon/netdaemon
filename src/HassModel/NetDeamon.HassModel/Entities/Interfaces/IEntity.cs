namespace NetDaemon.HassModel.Entities;

public interface IEntity
{
    public string EntityId { get; }
    
    public IHaContext? HaContext { get; }

    public IObservable<StateChange> StateAllChanges();
}