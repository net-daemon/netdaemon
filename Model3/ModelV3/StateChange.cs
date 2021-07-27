namespace NetDaemon.Common.ModelV3
{
    public record StateChange()
    {
        public StateChange(Entity entity, EntityState old, EntityState @new) : this()
        {
            New = @new;
            Old = old;
            Entity = entity;
        }

        public virtual EntityState New { get; }
        public virtual EntityState Old { get; }
        public virtual Entity Entity { get; }
    }

    public record StateChange<TEntity, TEntityState> : StateChange where TEntity : Entity   
    {
        public override TEntity Entity { get; }
    }
}