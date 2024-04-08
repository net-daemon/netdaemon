namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Provides data from the HA Entity Registry regarding an Entity
/// </summary>
public record EntityRegistration
{
    public Area? Area { get; init; }
    public Device? Device { get; init; }

    public IReadOnlyCollection<Label> Labels { get; init; } = Array.Empty<Label>();
}
