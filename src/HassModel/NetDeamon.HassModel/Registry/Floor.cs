namespace NetDaemon.HassModel.Entities;

public class Floor
{
    private readonly IHaRegistry _registry;

    public Floor(IHaRegistry registry)
    {
        _registry = registry;
    }

    public string Name { get; init; }
    public string Id { get; init; }
    public string Icon { get; init; }
    public string Description { get; init; }
    public string Color { get; init; }

    public IEnumerable<Area> Entities => _registry.GetAreasForFloor(this);
}
