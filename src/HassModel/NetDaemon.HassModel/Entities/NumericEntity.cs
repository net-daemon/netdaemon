namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Entity that has a numeric (double) State value
/// </summary>
public record NumericEntity : Entity
{
    /// <summary>Copy constructor from base class</summary>
    public NumericEntity(IEntityCore entity) : base(entity) { }
    
    /// <summary>Constructor from haContext and entityId</summary>
    public NumericEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
        
    /// <summary>The current state of this Entity converted to double if possible, null if it is not</summary>
    public new double? State => EntityState?.State;

    /// <inheritdoc/>
    public override NumericEntityState? EntityState => base.EntityState == null ? null : new (base.EntityState);
        
    /// <inheritdoc/>
    public override IObservable<NumericStateChange> StateAllChanges() => 
        base.StateAllChanges().Select(e => new NumericStateChange(this, 
            Entities.EntityState.Map<NumericEntityState>(e.Old),
            Entities.EntityState.Map<NumericEntityState>(e.New)));
        
    /// <inheritdoc/>
    public override IObservable<NumericStateChange> StateChanges() => StateAllChanges().StateChangesOnly();
}
    
/// <summary>
/// Entity that has a numeric (double) State value
/// </summary>
public record NumericEntity<TEntity, TEntityState, TAttributes> : Entity<TEntity, TEntityState, TAttributes>
    where TEntity : NumericEntity<TEntity, TEntityState, TAttributes>
    where TEntityState : NumericEntityState<TAttributes>
    where TAttributes : class
{
    /// <summary>Copy constructor from base class</summary>
    public NumericEntity(IEntityCore entity) : base(entity) { }

    /// <summary>Constructor from haContext and entityId</summary>
    public NumericEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
        
    /// <summary>The current state of this Entity converted to double if possible, null if it is not</summary>
    public new double? State => EntityState?.State;
        
    /// <summary>The full state of this Entity</summary>
    public new NumericEntityState<TAttributes>? EntityState => base.EntityState == null ? null : new (base.EntityState);
    // we need a new here because EntityState is not covariant for TAttributes

    /// <inheritdoc/>
    public override IObservable<NumericStateChange<TEntity, TEntityState>> StateAllChanges() => 
        base.StateAllChanges().Select(e => new NumericStateChange<TEntity, TEntityState>((TEntity)this, e.Old, e.New));

    /// <inheritdoc/>
    public override IObservable<NumericStateChange<TEntity, TEntityState>> StateChanges() =>
        StateAllChanges().StateChangesOnly();
}
    
/// <summary>
/// Entity that has a numeric (double) State value
/// </summary>
public record NumericEntity<TAttributes> : NumericEntity<NumericEntity<TAttributes>, NumericEntityState<TAttributes>, TAttributes>
    where TAttributes : class
{
    // This type is needed because the base type has a recursive type parameter so it can not be used as a return value
        
    /// <summary>Copy constructor from base class</summary>
    public NumericEntity(Entity entity) : base(entity) { }
    
    /// <summary>Constructor from haContext and entityId</summary>
    public NumericEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
}