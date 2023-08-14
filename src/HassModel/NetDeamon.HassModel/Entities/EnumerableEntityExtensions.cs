namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Provides extension methods for IEnumerable&lt;Entity&gt;
/// </summary>
public static class EnumerableEntityExtensions
{
    /// <summary>
    /// Observable that emits all state changes, including attribute changes.<br/>
    /// Use <see cref="System.ObservableExtensions.Subscribe{T}(System.IObservable{T})"/> to subscribe to the returned observable and receive state changes.
    /// </summary>
    /// <example>
    /// <code>
    /// bedroomLights.StateAllChanges()
    ///     .Where(s =&gt; s.Old?.Attributes?.Brightness &lt; 128 
    ///              &amp;&amp; s.New?.Attributes?.Brightness &gt;= 128)
    ///     .Subscribe(e =&gt; HandleBrightnessOverHalf());
    /// </code>
    /// </example>
    public static IObservable<StateChange> StateAllChanges(this IEnumerable<Entity> entities) => 
        entities.Select(t => t.StateAllChanges()).Merge();

    /// <summary>
    /// Observable that emits state changes where New.State != Old.State<br/>
    /// Use <see cref="System.ObservableExtensions.Subscribe{T}(System.IObservable{T})"/> to subscribe to the returned observable and receive state changes.
    /// </summary>
    /// <example>
    /// <code>
    /// disabledLights.StateChanges()
    ///    .Where(s =&gt; s.New?.State == "on")
    ///    .Subscribe(e =&gt; e.Entity.TurnOff());
    /// </code>
    /// </example>
    public static IObservable<StateChange> StateChanges(this IEnumerable<Entity> entities) =>
        entities.StateAllChanges().StateChangesOnly();
        
    /// <summary>
    /// Observable that emits all state changes, including attribute changes.<br/>
    /// Use <see cref="System.ObservableExtensions.Subscribe{T}(System.IObservable{T})"/> to subscribe to the returned observable and receive state changes.
    /// </summary>
    /// <example>
    /// <code>
    /// bedroomLights.StateAllChanges()
    ///     .Where(s =&gt; s.Old?.Attributes?.Brightness &lt; 128 
    ///              &amp;&amp; s.New?.Attributes?.Brightness &gt;= 128)
    ///     .Subscribe(e =&gt; HandleBrightnessOverHalf());
    /// </code>
    /// </example>
    public static IObservable<StateChange<TEntity, TEntityState>> StateAllChanges<TEntity, TEntityState, TAttributes>(this IEnumerable<Entity<TEntity, TEntityState, TAttributes>> entities) 
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class =>
        entities.Select(t => t.StateAllChanges()).Merge();

    /// <summary>
    /// Observable that emits state changes where New.State != Old.State<br/>
    /// Use <see cref="System.ObservableExtensions.Subscribe{T}(System.IObservable{T})"/> to subscribe to the returned observable and receive state changes.
    /// </summary>
    /// <example>
    /// <code>
    /// disabledLights.StateChanges()
    ///    .Where(s =&gt; s.New?.State == "on")
    ///    .Subscribe(e =&gt; e.Entity.TurnOff());
    /// </code>
    /// </example>
    public static IObservable<StateChange<TEntity, TEntityState>> StateChanges<TEntity, TEntityState, TAttributes>(this IEnumerable<Entity<TEntity, TEntityState, TAttributes>> entities) 
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class => 
        entities.StateAllChanges().StateChangesOnly();

    /// <summary>
    /// Calls a service with a set of Entities as the target
    /// </summary>
    /// <param name="entities">IEnumerable of Entities for which to call the service</param>
    /// <param name="service">Name of the service to call. If the Domain of the service is the same as the domain of the Entities it can be omitted</param>
    /// <param name="data">Data to provide</param>
    public static void CallService(this IEnumerable<IEntityCore> entities, string service, object? data = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        
        entities = entities.ToList();
        
        if (!entities.Any()) return;
        
        var (serviceDomain, serviceName) = service.SplitAtDot();

        if (serviceDomain == null)
        {
            var domainsFromEntity = entities.Select(e => e.EntityId.SplitAtDot().Left).Distinct().Take(2).ToArray();
            if (domainsFromEntity.Length != 1) throw new InvalidOperationException($"Cannot call service {service} for entities that do not have the same domain");
            
            serviceDomain = domainsFromEntity.First()!;
        }
        
        // Usually each Entity will have the same IHaContext and domain, but just in case its not, group by the context and domain and call the
        // service for each group separately
        var serviceCalls = entities.GroupBy(e => e.HaContext);
        
        foreach (var group in serviceCalls)
        {
            group.Key.CallService(serviceDomain, serviceName, new ServiceTarget { EntityIds = group.Select(e => e.EntityId).ToList() }, data);
        }
    }
}