namespace NetDaemon.HassModel.Entities;

public class Floor
{
    private readonly IHaRegistry _registry;

    internal Floor(IHaRegistry registry)
    {
        _registry = registry;
    }

    public string? Name { get; init; }
    public string? Id { get; init; }
    public string? Icon { get; init; }
    public short? Level { get; init; }

    public IEnumerable<Area> Areas => _registry.GetAreasForFloor(this);
}
