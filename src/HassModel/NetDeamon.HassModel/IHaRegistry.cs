namespace NetDaemon.HassModel;

public interface IHaRegistry
{
    IReadOnlyCollection<EntityRegistration> Entities { get; }
    IReadOnlyCollection<Device> Devices { get; }
    IReadOnlyCollection<Area> Areas { get; }
    IReadOnlyCollection<Floor> Floors { get; }
    IReadOnlyCollection<Label> Labels { get; }

    EntityRegistration? GetEntityRegistration(string entityId);
    Device? GetDevice(string deviceId);
    Area? GetArea(string areaId);
    Floor? GetFloor(string floorId);
    Label? GetLabel(string labelId);
}
