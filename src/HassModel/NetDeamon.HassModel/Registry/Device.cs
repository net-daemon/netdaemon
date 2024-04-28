namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Details of a Device in the Home Assistant Registry
/// </summary>
public record Device
{
    private readonly IHaRegistryNavigator _registry;

    internal Device(IHaRegistryNavigator registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// The Name of this Device
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The Id of this Device
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The Area of this Device
    /// </summary>
    public Area? Area { get; init; }

    /// <summary>
    /// The Entities of this Device
    /// </summary>
    public IReadOnlyCollection<Entity> Entities => _registry.GetEntitiesForDevice(this).ToList();

    /// <summary>
    /// The Labels of this Device
    /// </summary>
    public IReadOnlyCollection<Label> Labels { get; init; } = Array.Empty<Label>();
}
