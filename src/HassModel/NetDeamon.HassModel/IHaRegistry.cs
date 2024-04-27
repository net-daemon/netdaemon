namespace NetDaemon.HassModel;

/// <summary>
/// Provides access to objects from the Homa Assistant Registry
/// </summary>
public interface IHaRegistry
{
    /// <summary>
    /// Retrieves all EntityRegistrations from the Home Assistant registry
    /// </summary>
    IReadOnlyCollection<EntityRegistration> Entities { get; }

    /// <summary>
    /// Retrieves all EntityRegistrations from the Home Assistant registry
    /// </summary>
    IReadOnlyCollection<Device> Devices { get; }

    /// <summary>
    /// Retrieves all Areas from the Home Assistant registry
    /// </summary>
    IReadOnlyCollection<Area> Areas { get; }

    /// <summary>
    /// Retrieves all Floors from the Home Assistant registry
    /// </summary>
    IReadOnlyCollection<Floor> Floors { get; }

    /// <summary>
    /// Retrieves all Labels from the Home Assistant registry
    /// </summary>
    IReadOnlyCollection<Label> Labels { get; }

    /// <summary>
    /// Retrieves the EntityRegistration for an Entity by its entityId
    /// </summary>
    EntityRegistration? GetEntityRegistration(string entityId);

    /// <summary>
    /// Retrieves a Device by its Id
    /// </summary>
    Device? GetDevice(string deviceId);

    /// <summary>
    /// Retrieves an Area  by its Id
    /// </summary>
    Area? GetArea(string areaId);

    /// <summary>
    /// Retrieves a Floorby its Id
    /// </summary>
    Floor? GetFloor(string floorId);

    /// <summary>
    /// Retrieves a Label  by its Id
    /// </summary>
    Label? GetLabel(string labelId);
}
