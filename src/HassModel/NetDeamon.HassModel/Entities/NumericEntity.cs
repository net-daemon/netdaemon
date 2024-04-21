namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Entity that has a numeric (double) State value
/// </summary>
public record NumericEntity<TAttributes> : Entity<NumericEntity<TAttributes>, EntityState<double?, TAttributes>, TAttributes, double?>
   where TAttributes : class
{
    /// <summary>Copy constructor from base class</summary>
    public NumericEntity(IEntityCore entity) : base(entity) { }

    /// <summary>Constructor from haContext and entityId</summary>
    public NumericEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }

    /// <summary>The current state of this Entity converted to double if possible, null if it is not</summary>
    public new double? State => EntityState?.State;

    /// <inheritdoc/>
    public override EntityState<double?, TAttributes>? EntityState => base.EntityState == null ? null : new (base.EntityState);

    /// <summary>Gets a new Entity from this Entity with the specified type of attributes</summary>
    public new NumericEntity<T> WithAttributesAs<T>()
        where T : class
        => new(this);
}

public record NumericEntity : Entity<NumericEntity, EntityState<double?, object>, object, double?>
{
    public NumericEntity(IEntityCore entity) : base(entity)
    {
    }

    public NumericEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public new NumericEntity<T> WithAttributesAs<T>()
        where T : class
        => new(this);
}
