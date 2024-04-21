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
