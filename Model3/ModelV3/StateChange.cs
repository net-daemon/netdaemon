namespace NetDaemon.Common.ModelV3
{
    public record StateChange()
    {
        public StateChange(Entity entity, EntityState? old, EntityState? @new) : this()
        {
            New = @new;
            Old = old;
            Entity = entity;
        }

        public virtual Entity Entity { get; }
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