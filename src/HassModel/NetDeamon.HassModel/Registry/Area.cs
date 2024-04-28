namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Details of aa Area in the Home Assistant Registry
/// </summary>
public record Area
{
    private readonly IHaRegistryNavigator _registry;

    internal Area(IHaRegistryNavigator registry)
    {
        _registry = registry;
    }
    /// <summary>
    /// The area's name
    /// </summary>
    public string? Name { get; init; }
    /// <summary>
    /// The area's Id
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// The area's Floor
    /// </summary>
    public Floor? Floor { get; init; }

    /// <summary>
    /// The Devices in this Area
    /// </summary>
    public IReadOnlyCollection<Device> Devices => _registry.GetDevicesForArea(this).ToList();

    /// <summary>
    /// The Entities in this Area (either direct or via their Device)
    /// </summary>
    public IReadOnlyCollection<Entity> Entities => _registry.GetEntitiesForArea(this).ToList();

    /// <summary>
    /// The Labels of this Area
    /// </summary>
    public IReadOnlyCollection<Label> Labels { get; init; } = Array.Empty<Label>();
}
