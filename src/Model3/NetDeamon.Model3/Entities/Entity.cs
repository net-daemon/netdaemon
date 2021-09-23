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

        /// <summary>
        /// Creates a new instance of a Entity class
        /// </summary>
        /// <param name="haContext">The Home Assisatnt context associated with this Entity</param>
        /// <param name="entityId">The id of this Entity</param>
        public Entity(IHaContext haContext, string entityId)
        {
            HaContext = haContext;
            EntityId = entityId;
        }

        // temporary suppress, this will go away when implemented
        [SuppressMessage("", "CA1822")]
        public string Area => "todo";

        /// <summary>
        /// The current state of this Entity 
        /// </summary>
        public string? State => EntityState?.State;

        /// <summary>
        /// The current Attributes of this Entity
        /// </summary>
        public virtual object? Attributes => EntityState?.Attributes;

        /// <summary>
        /// The full state of this Entity
        /// </summary>
        public virtual EntityState? EntityState => HaContext.GetState(EntityId);


        /// <summary>
        /// Observable, All state changes including attributes
        /// </summary>
        public virtual IObservable<StateChange> StateAllChanges =>
            HaContext.StateAllChanges.Where(e => e.Entity.EntityId == EntityId)
                .Select(e => new StateChange(this, e.Old, e.New));

        /// <summary>
        /// Observable, All state changes. New.State!=Old.State
        /// </summary>
        public virtual IObservable<StateChange> StateChanges =>
            StateAllChanges.Where(s => s.New?.State != s.Old?.State)
                .Select(e => new StateChange(this, e.Old, e.New));

        /// <summary>
        /// Calls a service using this entity as the target
        /// </summary>
        /// <param name="service">Name of the service to call</param>
        /// <param name="data">Data to provide</param>
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

        /// <inheritdoc />
        public new TState? State => base.State == null ? default : (TState?)Convert.ChangeType(base.State, typeof(TState), CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public override TAttributes? Attributes => _attributesLazy.Value;

        /// <inheritdoc />
        public override TEntityState? EntityState => MapNullableState(base.EntityState);

        /// <inheritdoc />
        public override IObservable<StateChange<TEntity, TEntityState>> StateAllChanges =>
            base.StateAllChanges.Select(e => new StateChange<TEntity, TEntityState>((TEntity)this, MapNullableState(e.Old), MapNullableState(e.New)));

        /// <inheritdoc />
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
