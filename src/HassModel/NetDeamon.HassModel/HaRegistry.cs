namespace NetDaemon.HassModel;

internal class HaRegistry : IHaRegistry
{
    private readonly RegistryCache _registryCache;

    public HaRegistry(RegistryCache registryCache)
    {
        _registryCache = registryCache;
    }

    public EntityRegistration? GetEntityRegistration(string entityId)
    {
        var hassEntity = _registryCache.GetHassEntityById(entityId);
        if (hassEntity is null) return null;

        return BuildEntityRegistration(hassEntity);
    }


    public IEnumerable<EntityRegistration> GetEntitiesForArea(Area area)
    {
        return _registryCache.GetEntitiesForArea(area.Id).Select(e => BuildEntityRegistration(e, area: area));
    }

    public IEnumerable<Device> GetDevicesForArea(Area area)
    {
        return _registryCache.GetDevicesForArea(area.Id).Select(hd => hd.Map(this, area));
    }

    public IEnumerable<EntityRegistration> GetEntitiesForDevice(Device device)
    {
        return _registryCache.GetEntitiesForDevice(device.Id).Select(e => BuildEntityRegistration(e, device:device));
    }

    public IEnumerable<EntityRegistration> GetEntitiesForLabel(Label label)
    {
        return _registryCache.GetEntitiesForLabel(label.Id).Select(e=>BuildEntityRegistration(e));
    }

    private EntityRegistration BuildEntityRegistration(HassEntity hassEntity, Area? area = null, Device? device = null)
    {
        area ??= _registryCache.GetAreaForEntity(hassEntity)?.Map(this);
        device ??= _registryCache.GetDeviceById(hassEntity.DeviceId)?.Map(this, area);

        return new EntityRegistration(this, area, device)
        {
            Labels = _registryCache.GetLabelsForEntity(hassEntity.EntityId).Select(l => l.Map(this)).ToList(),
        };
    }
}
