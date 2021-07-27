#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Reactive.Linq;
using Model3;
using NetDaemon.Common.ModelV3.Domains;

namespace NetDaemon.Common.ModelV3
{
    /// <summary>
    /// Represents a Home Assitant entity with its state, changes and services
    /// </summary>
    public class Entity
    {
        /// <summary>
        ///     The IHAContext
        /// </summary>
        protected IHaContext HaContext { get; }

        /// <summary>
        ///     Entity id being handled by this entity
        /// </summary>
        public string EntityId { get; }

        public Entity(IHaContext daemon, string entityId)
        {
            HaContext = daemon;
            EntityId = entityId;
        }

        public virtual EntityState? State => HaContext.GetState(EntityId);

        public virtual IObservable<StateChange> StateAllChanges =>
            HaContext.StateAllChanges.Where(e => e.New.EntityId == EntityId)
                .Select(e => new StateChange(this, e.Old, e.New));

        public IObservable<StateChange> StateChanges =>
            StateAllChanges.Where(s => s.New.State != s.Old.State).Select(e => new StateChange(this, e.Old, e.New));

        public virtual void CallService(string service, object? data = null, bool waitForResponse = false)
        {
            var domain = EntityId.SplitEntityId().Domain;

            var serviceData = new
            {
                target = new
                {
                    entity_id = EntityId
                },
                data,
            };
            
            HaContext.CallService(domain, service, serviceData, waitForResponse);
        }
    }

}
