using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace NetDaemon.HassModel.Entities
{
    /// <summary>
    /// Provides extension methods for IEnumerable&lt;Entity&gt;
    /// </summary>
    public static class EnumerableEntityExtensions
    {
        /// <summary>
        /// Observable, All state changes including attributes
        /// </summary>
        public static IObservable<StateChange> StateAllChanges(this IEnumerable<Entity> entities) => 
            entities.Select(t => t.StateAllChanges()).Merge();

        /// <summary>
        /// Observable, All state changes. New.State != Old.State
        /// </summary>
        public static IObservable<StateChange> StateChanges(this IEnumerable<Entity> entities) =>
            entities.StateAllChanges().StateChangesOnly();
        
        /// <summary>
        /// Observable, All state changes including attributes
        /// </summary>
        public static IObservable<StateChange<TEntity, TEntityState>> StateAllChanges<TEntity, TEntityState, TAttributes>(this IEnumerable<Entity<TEntity, TEntityState, TAttributes>> entities) 
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class =>
            entities.Select(t => t.StateAllChanges()).Merge();

        /// <summary>
        /// Observable, All state changes. New.State != Old.State
        /// </summary>
        public static IObservable<StateChange<TEntity, TEntityState>> StateChanges<TEntity, TEntityState, TAttributes>(this IEnumerable<Entity<TEntity, TEntityState, TAttributes>> entities) 
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class => 
            entities.StateAllChanges().StateChangesOnly();
        
        /// <summary>
        /// Calls a service with a set of Entities as the target
        /// </summary>
        public static void CallService(this IEnumerable<Entity> entities, string domain, string service, object? data)
        {
            // Usually each Entity will have the same IHaContext, but just in case group by the context and call the
            // service for each context separately
            
            var perContext = entities.GroupBy(e => e.HaContext);
            foreach (var group in perContext)
            {
                group.Key.CallService(domain, service, new ServiceTarget { EntityIds = group.Select(e => e.EntityId).ToList() }, data);
            }
        }
    }
}
