#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive.Linq;
using NetDaemon.Model3.Common;

namespace NetDaemon.Model3.Entities
{
    /// <summary>
    /// Represents a Home Assistant entity with its state, changes and services
    /// </summary>
    public record Entity
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

        // temporary supress, this will go away when implemented
        [SuppressMessage("", "CA1822")]
        public string Area => "todo";

        public string? State => EntityState?.State;

        public virtual object? Attributes => EntityState?.Attributes;

        public virtual EntityState? EntityState => HaContext.GetState(EntityId);


        public virtual IObservable<StateChange> StateAllChanges =>
            HaContext.StateAllChanges.Where(e => e.Entity.EntityId == EntityId)
                .Select(e => new StateChange(this, e.Old, e.New));

        public virtual IObservable<StateChange> StateChanges =>
            StateAllChanges.Where(s => s.New?.State != s.Old?.State)
                .Select(e => new StateChange(this, e.Old, e.New));

        public virtual void CallService(string service, object? data = null)
        {
            HaContext.CallService(EntityId.SplitEntityId().Domain, service, data, this);
        }
    }

    public abstract record Entity<TEntity, TEntityState, TState, TAttributes> : Entity
        where TEntity : Entity<TEntity, TEntityState, TState, TAttributes>
        where TEntityState : EntityState<TState, TAttributes>
        where TAttributes : class
    {
        private readonly Lazy<TAttributes?> _attributesLazy;

        protected Entity(IHaContext haContext, string entityId) : base(haContext, entityId)
        {
            _attributesLazy = new(() => EntityState?.AttributesJson?.ToObject<TAttributes>());
        }

        // We need a 'new' here because the normal type of State is string and we cannot overload string with eg double
        // TODO: smarter conversion of string to TState to take into account 'Unavalable' etc
        public new TState? State => base.State == null ? default : (TState?)Convert.ChangeType(base.State, typeof(TState), CultureInfo.InvariantCulture);

        public override TAttributes? Attributes => _attributesLazy.Value;

        public override TEntityState? EntityState => MapNullableState(base.EntityState);

        public override IObservable<StateChange<TEntity, TEntityState>> StateAllChanges =>
            base.StateAllChanges.Select(e => new StateChange<TEntity, TEntityState>((TEntity)this, MapNullableState(e.Old), MapNullableState(e.New)));

        public override IObservable<StateChange<TEntity, TEntityState>> StateChanges =>
            base.StateChanges.Select(e => new StateChange<TEntity, TEntityState>((TEntity)this, MapNullableState(e.Old), MapNullableState(e.New)));

        private static TEntityState? MapNullableState(EntityState? state)
        {
            // TODO: this requires the TEntityState to have a copy ctor from EntityState,
            // maybe we could make this work without the copy ctor
            return state == null ? null : (TEntityState)Activator.CreateInstance(typeof(TEntityState), state)!;
        }
    }
}
