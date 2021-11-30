using System;

namespace NetDaemon.HassModel.Entities
{
    /// <summary>
    /// State for a Numeric Entity
    /// </summary>
    public record NumericEntityState : EntityState
    {
        /// <summary>Copy constructor from base class</summary>
        public NumericEntityState(EntityState source) : base(source) { }

        /// <summary>The state converted to double if possible, null if it is not</summary>
        public new double? State => double.TryParse(base.State, out var result) ? result : null;
    }

    /// <summary>
    /// State for a Numeric Entity with specific types of Attributes
    /// </summary>
    public record NumericEntityState<TAttributes> : EntityState<TAttributes>
        where TAttributes : class
    {
        /// <summary>Copy constructor from base class</summary>
        public NumericEntityState(EntityState source) : base(source)
        { }

        /// <summary>The state converted to double if possible, null if it is not</summary>
        public new double? State => double.TryParse(base.State, out var result) ? result : null;
    }
    
    public record NumericStateChange : StateChange<NumericEntity, NumericEntityState> 
    {
        internal NumericStateChange(StateChange e) : base(new NumericEntity(e.Entity), e.Old == null ? null : new NumericEntityState(e.Old), e.New == null ? null : new NumericEntityState(e.New))
        { }
    }
    
    public record NumericStateChange<TEntity, TEntityState> : StateChange<TEntity, TEntityState> 
        where TEntity : Entity
        where TEntityState : EntityState
    {
        internal NumericStateChange(StateChange<TEntity, TEntityState> e) : base(e)
        { }
    }
}