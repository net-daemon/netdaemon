namespace NetDaemon.Model3.Entities
{
    public record StateChange()
    {
        public StateChange(Entity entity, EntityState? old, EntityState? @new) : this()
        {
            Entity = entity;
            New = @new;
            Old = old;
        }

        public virtual Entity Entity { get; } = default!; // Somehow this is needed to avoid a warning about this field being initialized
        public virtual EntityState? Old { get; }
        public virtual EntityState? New { get; }
    }

    public record StateChange<TEntity, TEntityState> : StateChange 
        where TEntity : Entity 
        where TEntityState : EntityState
    {
        public StateChange(TEntity entity, TEntityState? old, TEntityState? @new) : base(entity, old, @new)
        {
            Entity = entity;
            Old = old;
            New = @new;
        }
        
        public override TEntity Entity { get; }
        public override TEntityState? New { get; }
        public override TEntityState? Old { get; }
    }
}