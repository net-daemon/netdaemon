using System.Reflection.Metadata;

namespace NetDaemon.HassModel.Entities;

public record Label
{
    private readonly IHaRegistry _registry;

    public Label(IHaRegistry registry)
    {
        _registry = registry;
    }

    public string Name { get; init; }
    public string Id { get; init; }
    public string Icon { get; init; }
    public string Description { get; init; }
    public string Color { get; init; }

    public IEnumerable<EntityRegistration> Entities => _registry.GetEntitiesForLabel(this);
}
