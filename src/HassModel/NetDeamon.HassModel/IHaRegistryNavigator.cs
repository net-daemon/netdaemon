namespace NetDaemon.HassModel;

internal interface IHaRegistryNavigator : IHaRegistry
{
    IEnumerable<Device> GetDevicesForArea(Area area);
    IEnumerable<Entity> GetEntitiesForArea(Area area);
    IEnumerable<Entity> GetEntitiesForDevice(Device device);
    IEnumerable<Entity> GetEntitiesForLabel(Label label);
    IEnumerable<Area> GetAreasForFloor(Floor floor);
}
