namespace NetDaemon.HassModel.Entities;

/// <summary>
/// Details of an Entity  in the Home Assistant Registry
/// </summary>
public record EntityRegistration
{
    /// <summary>
    /// The Entities Id
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// The Name of this Entity
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The Area of this Entity
    /// </summary>
    public Area? Area { get; init; }

    /// <summary>
    /// The Device of this Entity
    /// </summary>
    public Device? Device { get; init; }

    /// <summary>
    /// The Labels of this Entity
    /// </summary>
    public IReadOnlyCollection<Label> Labels { get; init; } = Array.Empty<Label>();
}
