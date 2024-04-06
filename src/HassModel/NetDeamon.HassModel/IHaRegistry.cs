namespace NetDaemon.HassModel;

public interface IHaRegistry
{
    EntityRegistration GetEntityRegistration(string entityId);
    IEnumerable<Device> GetDevicesForArea(Area area);
    IEnumerable<EntityRegistration> GetEntitiesForArea(Area area);
    IEnumerable<EntityRegistration> GetEntitiesForDevice(Device device);
    IEnumerable<EntityRegistration> GetEntitiesForLabel(Label label);
}
