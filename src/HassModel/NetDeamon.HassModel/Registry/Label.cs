namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Details of a Label in the Home Assistant Registry
/// </summary>
public record Label
{
    private readonly IHaRegistryNavigator _registry;

    internal Label(IHaRegistryNavigator registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// The Name of this Label
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The Id of this Label
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The icon of this Label
    /// </summary>
    public string? Icon { get; init; }

    /// <summary>
    /// The Description of this Label
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The Color of this Label
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// All entities that have this label
    /// </summary>
    public IEnumerable<Entity> Entities => _registry.GetEntitiesForLabel(this);
}
