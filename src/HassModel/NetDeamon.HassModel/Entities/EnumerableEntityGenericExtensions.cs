namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Provides extension methods for IEnumerable&lt;Entity&gt;
/// </summary>
public static class EnumerableEntityGenericExtensions
{
    // /// <summary>
    // /// Observable, All state changes including attributes
    // /// </summary>
    // public static IObservable<IStateChange> RawStateAllChanges(this IEnumerable<IEntity> entities) => 
    //     entities.Select(t => t.RawStateAllChanges()).Merge();

    // /// <summary>
    // /// Observable, All state changes. New.State != Old.State
    // /// </summary>
    // public static IObservable<IStateChange> StateChanges(this IEnumerable<IEntity> entities) =>
    //     entities.RawStateAllChanges().StateChangesOnlyGeneric();
        
    // /// <summary>
    // /// Observable, All state changes including attributes
    // /// </summary>
    // public static IObservable<IStateChange<TState, TAttributes>> StateAllChanges<TState, TAttributes>(this IEnumerable<IEntity<TState, TAttributes>> entities)
    //     where TAttributes : class =>
    //     entities.Select(t => t.StateAllChanges()).Merge();

    // /// <summary>
    // /// Observable, All state changes. New.State != Old.State
    // /// </summary>
    // public static IObservable<IStateChange<TState, TAttributes>> StateChanges<TState, TAttributes>(this IEnumerable<IEntity<TState, TAttributes>> entities)
    //     where TAttributes : class => 
    //     entities.StateAllChanges().StateChangesOnlyGeneric();

    /// <summary>
    /// Calls a service with a set of Entities as the target
    /// </summary>
    /// <param name="entities">IEnumerable of Entities for which to call the service</param>
    /// <param name="service">Name of the service to call. If the Domain of the service is the same as the domain of the Entities it can be omitted</param>
    /// <param name="data">Data to provide</param>
    public static void CallService<TState, TAttributes>(this IEnumerable<IEntity<TState, TAttributes>> entities, string service, object? data = null)
        where TAttributes : class
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