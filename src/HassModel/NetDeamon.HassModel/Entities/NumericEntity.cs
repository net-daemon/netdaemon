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
}

/// <summary>
/// Entity that has a numeric (double) State value
/// </summary>
public record NumericEntity<TEntityState, TAttributes> : Entity<TEntityState, TAttributes>
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
}

/// <summary>
/// Entity that has a numeric (double) State value
/// </summary>
public record NumericEntity<TAttributes> : NumericEntity<NumericEntityState<TAttributes>, TAttributes>
    where TAttributes : class
{
    // This type is needed because the base type has a recursive type parameter so it can not be used as a return value

    /// <summary>Copy constructor from base class</summary>
    public NumericEntity(Entity entity) : base(entity) { }

    /// <summary>Constructor from haContext and entityId</summary>
    public NumericEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
}
