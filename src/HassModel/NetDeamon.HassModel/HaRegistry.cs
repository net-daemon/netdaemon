namespace NetDaemon.HassModel;

internal class HaRegistry : IHaRegistry
{
    private readonly IHaContext _haContext;
    private readonly RegistryCache _registryCache;

    public HaRegistry(IHaContext haContext, RegistryCache registryCache)
    {
        _haContext = haContext;
        _registryCache = registryCache;
    }

    public EntityRegistration? GetEntityRegistration(string entityId)
    {
        var hassEntity = _registryCache.GetHassEntityById(entityId);
        if (hassEntity is null) return null;

        return BuildEntityRegistration(hassEntity);
    }


    public IEnumerable<Entity> GetEntitiesForArea(Area area)
    {
        return _registryCache.GetEntitiesForArea(area.Id).Select(e => _haContext.Entity(e.EntityId!));
    }

    public IEnumerable<Device> GetDevicesForArea(Area area)
    {
        return _registryCache.GetDevicesForArea(area.Id).Select(hd => hd.Map(this, area));
    }

    public IEnumerable<Entity> GetEntitiesForDevice(Device device)
    {
        return _registryCache.GetEntitiesForDevice(device.Id).Select(e => _haContext.Entity(e.EntityId!));
    }

    public IEnumerable<Entity> GetEntitiesForLabel(Label label)
    {
        return _registryCache.GetEntitiesForLabel(label.Id).Select(e => _haContext.Entity(e.EntityId!));
    }

    public Label GetLabelById(string labelId)
    {
        return _registryCache.GetLabelById(labelId).Map(this);
    }

    public IEnumerable<Area> GetAreasForFloor(Floor floor)
    {
        return _registryCache.GetAreasForFloor(floor.Id).Map(this);
    }

    private EntityRegistration BuildEntityRegistration(HassEntity hassEntity, Area? area = null, Device? device = null)
    {
        area ??= _registryCache.GetAreaForEntity(hassEntity)?.Map(this);
        device ??= _registryCache.GetDeviceById(hassEntity.DeviceId)?.Map(this, area);

        return new EntityRegistration(this)
        {
            Area = area,
            Device = device,
            Labels = _registryCache.GetLabelsForEntity(hassEntity.EntityId).Select(l => l.Map(this)).ToList(),
        };
    }
}
