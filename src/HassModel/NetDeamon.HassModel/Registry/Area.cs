namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Area detail class
/// </summary>
public record Area
{
    private readonly IHaRegistry _registry;

    internal Area(IHaRegistry registry)
    {
        _registry = registry;
    }
    /// <summary>
    /// The area's name
    /// </summary>
    public string? Name { get; init; }
    public string? Id { get; init; }

    public Floor? Floor { get; init; }

    public IReadOnlyCollection<Device> Devices => _registry.GetDevicesForArea(this).ToList();
    public IReadOnlyCollection<Entity> Entities => _registry.GetEntitiesForArea(this).ToList();

    public IReadOnlyCollection<Label> Labels { get; init; } = Array.Empty<Label>();
}
