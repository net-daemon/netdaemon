﻿namespace NetDaemon.HassModel.Entities
{
    /// <summary>
    /// Represents a state change event for an entity
    /// </summary>
    public record StateChange
    {
        public StateChange(Entity entity, EntityState? old, EntityState? @new) 
        {
            Entity = entity;
            New = @new;
            Old = old;
        }

        /// <summary>The Entity that changed</summary>
        public virtual Entity Entity { get; } = default!; // Somehow this is needed to avoid a warning about this field being initialized

        /// <summary>The old state of the entity</summary>
        public virtual EntityState? Old { get; }

        /// <summary>The new state of the entity</summary>
        public virtual EntityState? New { get; }
    }

    /// <summary>
    /// Represents a state change event for a strong typed entity and state 
    /// </summary>
    /// <typeparam name="TEntity">The Type</typeparam>
    /// <typeparam name="TEntityState"></typeparam>
    public record StateChange<TEntity, TEntityState> : StateChange 
        where TEntity : Entity 
        where TEntityState : EntityState
    {
        public StateChange(TEntity entity, TEntityState? old, TEntityState? @new) : base(entity, old, @new)
        { }

        /// <inheritdoc/>
        public override TEntity Entity => (TEntity)base.Entity;

        /// <inheritdoc/>
        public override TEntityState? New => (TEntityState?)base.New;

        /// <inheritdoc/>
        public override TEntityState? Old => (TEntityState?)base.Old;
    }
}
