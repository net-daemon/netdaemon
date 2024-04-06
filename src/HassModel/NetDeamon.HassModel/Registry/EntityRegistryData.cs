namespace NetDaemon.HassModel.Entities;

public record EntityRegistration
{
    internal EntityRegistration(IHaRegistry haRegistry, Area? area, Device? device)
    {
        this.haRegistry = haRegistry;
        Area = area;
        Device = device;
    }

    private IHaRegistry haRegistry;
    public Area? Area { get; init; }
    public Device? Device { get; init; }

    public IReadOnlyCollection<Label> Labels { get; init; } = Array.Empty<Label>();

}
