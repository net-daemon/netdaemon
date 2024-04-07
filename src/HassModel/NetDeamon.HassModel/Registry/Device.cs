namespace NetDaemon.HassModel.Entities;

public record Device
{
    private readonly IHaRegistry _registry;

    internal Device(IHaRegistry registry)
    {
        _registry = registry;
    }
    public string? Name { get; init; }
    public string Id { get; init; }
    public Area? Area { get; init; }

    public IReadOnlyCollection<Entity> Entities => _registry.GetEntitiesForDevice(this).ToList();

    public IReadOnlyCollection<Label> Labels { get; init; } = Array.Empty<Label>();
}
