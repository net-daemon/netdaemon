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

    /// <summary>
    /// The manufacturer of this Device, if available
    /// </summary>
    public string? Manufacturer { get; init; }

    /// <summary>
    /// The model of this Device, if available
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// A URL on which the device or service can be configured
    /// </summary>
    #pragma warning disable CA1056 // It's ok for this URL to be a string
    public string? ConfigurationUrl { get; init; }
    #pragma warning restore CA1056

    /// <summary>
    /// The hardware version of this Device, if available
    /// </summary>
    public string? HardwareVersion { get; init; }

    /// <summary>
    /// The software version of this Device, if available
    /// </summary>
    public string? SoftwareVersion { get; init; }

    /// <summary>
    /// The serial number of this Device, if available
    /// </summary>
    public string? SerialNumber { get; init; }
}
