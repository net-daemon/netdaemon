namespace NetDaemon.HassModel;

internal class HaRegistry(IHaContext haContext, RegistryCache registryCache) : IHaRegistryNavigator, IHaRegistry
{
    public IReadOnlyCollection<EntityRegistration> Entities => registryCache.GetEntities().Select(e => e.Map(this)).ToList();
    public IReadOnlyCollection<Device> Devices => registryCache.GetDevices().Select(d => d.Map(this)).ToList();
    public IReadOnlyCollection<Area> Areas => registryCache.GetAreas().Select(a => a.Map(this)).ToList();
    public IReadOnlyCollection<Floor> Floors => registryCache.GetFloors().Select(f => f.Map(this)).ToList();
    public IReadOnlyCollection<Label> Labels => registryCache.GetLabels().Select(f => f.Map(this)).ToList();


    public EntityRegistration? GetEntityRegistration(string? entityId) => registryCache.GetHassEntityById(entityId)?.Map(this);
    public Device? GetDevice(string? deviceId) => registryCache.GetDeviceById(deviceId)?.Map(this);
    public Area? GetArea(string? areaId) => registryCache.GetAreaById(areaId)?.Map(this);
    public Floor? GetFloor(string? floorId) => registryCache.GetFloorById(floorId)?.Map(this);
    public Label? GetLabel(string? labelId) => registryCache.GetLabelById(labelId)?.Map(this);


    public IEnumerable<Entity> GetEntitiesForArea(Area area) => registryCache.GetEntitiesForArea(area.Id).Select(e => haContext.Entity(e.EntityId!));
    public IEnumerable<Entity> GetEntitiesForDevice(Device device) => registryCache.GetEntitiesForDevice(device.Id).Select(e => haContext.Entity(e.EntityId!));
    public IEnumerable<Entity> GetEntitiesForLabel(Label label) => registryCache.GetEntitiesForLabel(label.Id).Select(e => haContext.Entity(e.EntityId!));
    public IEnumerable<Device> GetDevicesForArea(Area area) => registryCache.GetDevicesForArea(area.Id).Select(hd => hd.Map(this));
    public IEnumerable<Area> GetAreasForFloor(Floor floor) => registryCache.GetAreasForFloor(floor.Id).Select(a=>a.Map(this));
}
