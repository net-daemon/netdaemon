using System.Collections.ObjectModel;
using NetDaemon.Client.HomeAssistant.Extensions;

namespace NetDaemon.HassModel.Internal;

internal class RegistryCache(IHomeAssistantRunner hassRunner, ILogger<RegistryCache> logger) : IDisposable
{
    private CancellationToken _cancellationToken;
    private IDisposable? _eventSubscription;

    // Just in case this cache is used before it is initialized we will initialize all
    // these dictionaries and lookups empty to avoid exceptions
    private IReadOnlyCollection<HassEntity> _entities = new Collection<HassEntity>();

    private Dictionary<string, HassLabel> _labelsById = new ();
    private Dictionary<string, HassFloor> _floorsById = new();
    private Dictionary<string, HassArea> _areasById = new ();
    private Dictionary<string, HassDevice> _devicesById = new();
    private Dictionary<string, HassEntity> _entitesById = new ();

    private ILookup<string?, HassEntity> _entitiesByAreaId = Array.Empty<HassEntity>().ToLookup(e=>e.AreaId);
    private ILookup<string?, HassEntity> _entitiesByDeviceId = Array.Empty<HassEntity>().ToLookup(e=>e.DeviceId);
    private ILookup<string?, HassDevice> _devicesByAreaId = Array.Empty<HassDevice>().ToLookup(e=>e.AreaId);
    private ILookup<string?, HassArea> _areasByFloorId = Array.Empty<HassArea>().ToLookup(e => e.FloorId);

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        var events = await hassRunner.CurrentConnection!.SubscribeToHomeAssistantEventsAsync(null, _cancellationToken).ConfigureAwait(false);
        _eventSubscription = events.Subscribe(HandleEvent);

        await UpdateCache().ConfigureAwait(false);
    }

    private async Task UpdateCache()
    {
        logger.LogInformation("Updating RegistryCache");
        _ = hassRunner.CurrentConnection ?? throw new InvalidOperationException();

        _entities = await hassRunner.CurrentConnection.GetEntitiesAsync(_cancellationToken).ConfigureAwait(false) ?? Array.Empty<HassEntity>();
        _entitesById = _entities
            .Where(e => !string.IsNullOrEmpty(e.EntityId))
            .ToDictionary(e => e.EntityId!);

        var devices = await hassRunner.CurrentConnection.GetDevicesAsync(_cancellationToken).ConfigureAwait(false) ?? Array.Empty<HassDevice>();
        _devicesById = devices.ToDictionary(k => k.Id!, v => v);

        var areas = await hassRunner.CurrentConnection.GetAreasAsync(_cancellationToken).ConfigureAwait(false) ?? Array.Empty<HassArea>();
        _areasById = areas.ToDictionary(k => k.Id!, v => v);
        _areasByFloorId = areas.ToLookup(a => a.FloorId);

        var labels = await hassRunner.CurrentConnection.GetLabelsAsync(_cancellationToken).ConfigureAwait(false) ?? Array.Empty<HassLabel>();
        _labelsById = labels.ToDictionary(l => l.Id!);

        var floors = await hassRunner.CurrentConnection.GetFloorsAsync(_cancellationToken).ConfigureAwait(false) ?? Array.Empty<HassFloor>();
        _floorsById = floors.ToDictionary(f =>f.Id!);

        _entitiesByAreaId = _entities.ToLookup(FindArea);
        _entitiesByDeviceId = _entities.ToLookup(e => e.DeviceId);
        _devicesByAreaId = devices.ToLookup(d => d.AreaId);
    }

    public HassEntity? GetHassEntityById(string id) => _entitesById.GetValueOrDefault(id, null!);

    public IEnumerable<HassEntity> GetEntitiesForArea(string? areaId) => _entitiesByAreaId[areaId];

    public IEnumerable<HassEntity> GetEntitiesForDevice(string? deviceId) => _entitiesByDeviceId[deviceId];

    public IEnumerable<HassDevice> GetDevicesForArea(string? areaId) => _devicesByAreaId[areaId];

    public HassDevice? GetDeviceById(string? deviceId) =>
        deviceId is null ? null : _devicesById.GetValueOrDefault(deviceId, null!);

    public HassArea? GetAreaById(string? areaId) =>
        areaId is null ? null : _areasById.GetValueOrDefault(areaId, null!);

    public HassFloor? GetFloorById(string? floorId) =>
        floorId is null ? null : _floorsById.GetValueOrDefault(floorId, null!);

    public HassArea? GetAreaForEntity(HassEntity? entity) => GetAreaById(FindArea(entity));

    public IEnumerable<HassLabel> GetLabelsForEntity(string? entityId)
    {
        return (entityId is null ? null : _entitesById.GetValueOrDefault(entityId)?.Labels.Select(l =>  _labelsById[l]))
                ?? Array.Empty<HassLabel>();
    }

    public IEnumerable<HassEntity> GetEntitiesForLabel(string labelId) =>
        _entities.Where(e => e.Labels.Any(l => l == labelId));

    public HassLabel GetLabelById(string labelId) => _labelsById[labelId];

    public IEnumerable<HassArea> GetAreasForFloor(string? floorId) => _areasByFloorId[floorId];


    private string? FindArea(HassEntity? entity)
    {
        if (!string.IsNullOrEmpty(entity?.AreaId))
        {
            return entity.AreaId;
        }

        return entity?.DeviceId is null ? null : _devicesById.GetValueOrDefault(entity.DeviceId!)?.AreaId;
    }

    private void HandleEvent(HassEvent hassEvent)
    {
        if (hassEvent.EventType
            is "entity_registry_updated"
            or "device_registry_updated"
            or "area_registry_updated"
            or "label_registry_updated"
            or "floor_registry_updated")
            // Fire and forget
            _ = UpdateCache().ConfigureAwait(false);
    }

    public void Dispose()
    {
        _eventSubscription?.Dispose();
    }
}
