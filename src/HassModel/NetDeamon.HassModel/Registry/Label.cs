using System.Reflection.Metadata;

namespace NetDaemon.HassModel.Entities;

public record Label
{
    private readonly IHaRegistryNavigator _registry;

    internal Label(IHaRegistryNavigator registry)
    {
        _registry = registry;
    }

    public string? Name { get; init; }
    public string Id { get; init; }
    public string? Icon { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }

    public IEnumerable<Entity> Entities => _registry.GetEntitiesForLabel(this);
}
