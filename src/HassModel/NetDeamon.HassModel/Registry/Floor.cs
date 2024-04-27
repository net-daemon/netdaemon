namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Details of a Floor in the Home Assistant Registry
/// </summary>
public class Floor
{
    private readonly IHaRegistryNavigator _registry;

    internal Floor(IHaRegistryNavigator registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// The Name of this Floor
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The Id of this Floor
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// The Icon of this Floor
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// The Level of this Floor
    /// </summary>
    public short? Level { get; init; }

    /// <summary>
    /// The Areas of this Floor
    /// </summary>
    public IEnumerable<Area> Areas => _registry.GetAreasForFloor(this);
}
