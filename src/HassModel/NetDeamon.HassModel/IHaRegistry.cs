namespace NetDaemon.HassModel;

internal interface IHaRegistry
{
    EntityRegistration? GetEntityRegistration(string entityId);
    IEnumerable<Device> GetDevicesForArea(Area area);
    IEnumerable<Entity> GetEntitiesForArea(Area area);
    IEnumerable<Entity> GetEntitiesForDevice(Device device);
    IEnumerable<Entity> GetEntitiesForLabel(Label label);

    Label? GetLabelById(string labelId);
    IEnumerable<Area> GetAreasForFloor(Floor floor);
    Floor? GetFloorById(string? id);
    IEnumerable<Area> GetAreasForLabel(Label label);
}
