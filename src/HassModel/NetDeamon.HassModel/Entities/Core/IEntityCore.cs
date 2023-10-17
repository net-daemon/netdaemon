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

    /// <summary>The current state of this Entity</summary>
    public string? State { get; }

    /// <summary>
    /// The current Attributes of this Entity
    /// </summary>
    public object? Attributes { get; }

    /// <summary>
    /// The full state of this Entity
    /// </summary>
    public EntityState? EntityState { get; }

    /// <summary>
    /// Observable that emits all state changes, including attribute changes.<br/>
    /// Use <see cref="System.ObservableExtensions.Subscribe{T}(System.IObservable{T})"/> to subscribe to the returned observable and receive state changes.
    /// </summary>
    /// <example>
    /// <code>
    /// bedroomLight.StateAllChanges()
    ///     .Where(s =&gt; s.Old?.Attributes?.Brightness &lt; 128 
    ///              &amp;&amp; s.New?.Attributes?.Brightness &gt;= 128)
    ///     .Subscribe(e =&gt; HandleBrightnessOverHalf());
    /// </code>
    /// </example>
    public IObservable<StateChange> StateAllChanges();

    /// <summary>
    /// Observable that emits state changes where New.State != Old.State<br/>
    /// Use <see cref="System.ObservableExtensions.Subscribe{T}(System.IObservable{T})"/> to subscribe to the returned observable and receive state changes.
    /// </summary>
    /// <example>
    /// <code>
    /// disabledLight.StateChanges()
    ///    .Where(s =&gt; s.New?.State == "on")
    ///    .Subscribe(e =&gt; e.Entity.TurnOff());
    /// </code>
    /// </example>
    public IObservable<StateChange> StateChanges();

    /// <summary>
    /// Calls a service using this entity as the target
    /// </summary>
    /// <param name="service">Name of the service to call. If the Domain of the service is the same as the domain of the Entity it can be omitted</param>
    /// <param name="data">Data to provide</param>
    public void CallService(string service, object? data = null);
}

