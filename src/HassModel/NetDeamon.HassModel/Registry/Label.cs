using System.Reflection.Metadata;

namespace NetDaemon.HassModel.Entities;

public record Label
{
    private readonly IHaRegistry _registry;

    internal Label(IHaRegistry registry)
    {
        _registry = registry;
    }

    public string? Name { get; init; }
    public string Id { get; init; }
    public string? Icon { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }

    public IEnumerable<Entity> Entities => _registry.GetEntitiesForLabel(this);

    public IEnumerable<Area> Areas => _registry.GetAreasForLabel(this);
}
