namespace NetDaemon.HassModel.Entities;

/// <summary>
/// State for a Numeric Entity
/// </summary>
public record NumericEntityState : EntityState
{
    /// <summary>Copy constructor from base class</summary>
    public NumericEntityState(EntityState source) : base(source) { }

    /// <summary>The state converted to double if possible, null if it is not</summary>
    public new double? State => FormatHelpers.ParseAsDouble(base.State);
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
    public new double? State => FormatHelpers.ParseAsDouble(base.State);
}
    
/// <summary>
/// Represents a state change event for a strong typed entity and state 
/// </summary>
public record NumericStateChange : StateChange<NumericEntity, NumericEntityState> 
{
    internal NumericStateChange(NumericEntity entity, NumericEntityState? old, NumericEntityState? @new) : base(entity, old, @new)
    { }
}
    
/// <summary>
/// Represents a state change event for a strong typed entity and state 
/// </summary>
public record NumericStateChange<TEntity, TEntityState> : StateChange<TEntity, TEntityState> 
    where TEntity : Entity
    where TEntityState : EntityState
{
    internal NumericStateChange(TEntity entity, TEntityState? old, TEntityState? @new) : base(entity, old, @new)
    { }
}