namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Entity that has a numeric (double) State value
/// </summary>
public record NumericEntity : Entity<NumericEntity, EntityState<double, object>, object, double>
{
    /// <summary>Copy constructor from base class</summary>
    public NumericEntity(IEntityCore entity) : base(entity) { }

    /// <summary>Constructor from haContext and entityId</summary>
    public NumericEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }

    /// <summary>The current state of this Entity converted to double if possible, null if it is not</summary>
    public new double? State => EntityState?.State;

    /// <inheritdoc/>
    public override EntityState<double, object>? EntityState => base.EntityState == null ? null : new (base.EntityState);
}
