#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using NetDaemon.HassModel.Common;

namespace NetDaemon.HassModel.Entities
{
    /// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
    public record Entity
    {
        /// <summary>
        /// The IHAContext
        /// </summary>
        public IHaContext HaContext { get; }

        /// <summary>
        /// Entity id being handled by this entity
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        /// Creates a new instance of a Entity class
        /// </summary>
        /// <param name="haContext">The Home Assistant context associated with this Entity</param>
        /// <param name="entityId">The id of this Entity</param>
        public Entity(IHaContext haContext, string entityId)
        {
            HaContext = haContext;
            EntityId = entityId;
        }
        
        /// <summary>
        /// Area name of entity
        /// </summary>
        public string? Area => HaContext.GetAreaFromEntityId(EntityId)?.Name;

        /// <summary>The current state of this Entity</summary>
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
        public virtual IObservable<StateChange> StateAllChanges() =>
            HaContext.StateAllChanges().Where(e => e.Entity.EntityId == EntityId);

        /// <summary>
        /// Observable, All state changes. New.State!=Old.State
        /// </summary>
        public virtual IObservable<StateChange> StateChanges() =>
             StateAllChanges().StateChangesOnly();

        /// <summary>
        /// Calls a service using this entity as the target
        /// </summary>
        /// <param name="service">Name of the service to call. If the Domain of the service is the same as the domain of the Entity it can be omitted</param>
        /// <param name="data">Data to provide</param>
        public virtual void CallService(string service, object? data = null)
        {
            ArgumentNullException.ThrowIfNull(service, nameof(service));
            
            var (serviceDomain, serviceName) = service.SplitAtDot();

            serviceDomain ??= EntityId.SplitAtDot().Left ?? throw new InvalidOperationException("EntityId must be formatted 'domain.name'");
            
            HaContext.CallService(serviceDomain, serviceName, ServiceTarget.FromEntity(EntityId), data);
        }
    }

    /// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
    public abstract record Entity<TEntity, TEntityState, TAttributes> : Entity
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
    {
        /// <summary>Copy constructor from Base type</summary>
        protected Entity(Entity entity) : base(entity)
        { }

        /// <summary>Constructor from haContext and entityId</summary>
        protected Entity(IHaContext haContext, string entityId) : base(haContext, entityId)
        { }

        /// <inheritdoc />
        public override TAttributes? Attributes => EntityState?.Attributes;

        /// <inheritdoc />
        public override TEntityState? EntityState => MapState(base.EntityState);

        /// <inheritdoc />
        public override IObservable<StateChange<TEntity, TEntityState>> StateAllChanges() =>
            base.StateAllChanges().Select(e => new StateChange<TEntity, TEntityState>((TEntity)this, 
                Entities.EntityState.Map<TEntityState>(e.Old), 
                Entities.EntityState.Map<TEntityState>(e.New)));

        /// <inheritdoc />
        public override IObservable<StateChange<TEntity, TEntityState>> StateChanges() => StateAllChanges().StateChangesOnly();

        private static TEntityState? MapState(EntityState? state) => Entities.EntityState.Map<TEntityState>(state);
    }
    
    /// <summary>Represents a Home Assistant entity with its state, changes and services</summary>
    public record Entity<TAttributes> : Entity<Entity<TAttributes>, EntityState<TAttributes>, TAttributes>
        where TAttributes : class
    {
        // This type is needed because the base type has a recursive type parameter so it can not be used as a return value
        
        /// <summary>Copy constructor from Base type</summary>
        public Entity(Entity entity) : base(entity) { }
        
        /// <summary>Constructor from haContext and entityId</summary>
        public Entity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
    }
}
