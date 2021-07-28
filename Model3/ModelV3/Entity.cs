#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Reactive.Linq;
using Model3;

namespace NetDaemon.Common.ModelV3
{
    /// <summary>
    /// Represents a Home Assistant entity with its state, changes and services
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// The IHAContext
        /// </summary>
        protected IHaContext HaContext { get; }

        /// <summary>
        /// Entity id being handled by this entity
        /// </summary>
        public string EntityId { get; }

        public Entity(IHaContext haContext, string entityId)
        {
            HaContext = haContext;
            EntityId = entityId;
        }

        public string Area => "todo";

        public virtual EntityState? State => HaContext.GetState(EntityId);

        public virtual IObservable<StateChange> StateAllChanges =>
            HaContext.StateAllChanges.Where(e => e.New?.EntityId == EntityId)
                .Select(e => new StateChange(this, e.Old, e.New));

        public virtual IObservable<StateChange> StateChanges =>
            StateAllChanges.Where(s => s.New?.State != s.Old?.State)
                .Select(e => new StateChange(this, e.Old, e.New));

        public virtual void CallService(string service, object? data = null, bool waitForResponse = false)
        {
            // todo: use new method to pass target
            HaContext.CallService(EntityId.SplitEntityId().Domain, service, data , waitForResponse);
        }
    }
    
    public abstract class Entity<TEntity, TState> : Entity where TEntity:  Entity<TEntity, TState> where TState : EntityState
    {
        public Entity(IHaContext hasscontext, string entityId) : base(hasscontext, entityId)
        { }
    
        public override TState? State => MapNullableState(base.State) ;
    
        public override IObservable<StateChange<TEntity, TState>> StateAllChanges =>
            base.StateAllChanges.Select(e => new StateChange<TEntity, TState>((TEntity)this, MapNullableState(e.Old), MapNullableState(e.New)));

        public override IObservable<StateChange<TEntity, TState>> StateChanges =>
            base.StateChanges.Select(e => new StateChange<TEntity, TState>((TEntity)this, MapNullableState(e.Old), MapNullableState(e.New)));

        private TState? MapNullableState(EntityState? state) => state == null ? null : MapState(state!);
        protected abstract TState MapState(EntityState state);
    }

}
