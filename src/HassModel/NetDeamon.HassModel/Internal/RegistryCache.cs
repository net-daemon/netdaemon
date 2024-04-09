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
    private Dictionary<string, HassEntity> _entitiesById = new ();

    private ILookup<string?, HassEntity> _entitiesByAreaId = Array.Empty<HassEntity>().ToLookup(e=>e.AreaId);
    private ILookup<string?, HassEntity> _entitiesByDeviceId = Array.Empty<HassEntity>().ToLookup(e=>e.DeviceId);
    private ILookup<string?, HassDevice> _devicesByAreaId = Array.Empty<HassDevice>().ToLookup(e=>e.AreaId);
    private ILookup<string?, HassArea> _areasByFloorId = Array.Empty<HassArea>().ToLookup(e => e.FloorId);
    private ILookup<string, HassEntity> _entitiesByLabel = Array.Empty<HassEntity>().ToLookup(e =>default(string)!);

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        var events = await hassRunner.CurrentConnection!.SubscribeToHomeAssistantEventsAsync(null, _cancellationToken).ConfigureAwait(false);
        _eventSubscription = events.Where(e => e.EventType is
            "entity_registry_updated" or
            "device_registry_updated" or
            "area_registry_updated" or
            "label_registry_updated" or
            "floor_registry_updated")
            .Subscribe(e => { _ = UpdateCache(); }); // TODO, handel error sin async code

        await UpdateCache().ConfigureAwait(false);
    }

    private async Task UpdateCache()
    {
        logger.LogInformation("Updating RegistryCache");
        _ = hassRunner.CurrentConnection ?? throw new InvalidOperationException();

        _entities = await hassRunner.CurrentConnection.GetEntitiesAsync(_cancellationToken).ConfigureAwait(false) ?? Array.Empty<HassEntity>();
        _entitiesById = _entities
            .Where(e => !string.IsNullOrEmpty(e.EntityId))
            .ToDictionary(e => e.EntityId!);
        _entitiesByLabel = _entities.SelectMany(e => e.Labels.Select(l => (e, l))).ToLookup(t => t.l, t => t.e);

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

    public IEnumerable<HassEntity> GetEntities() => _entitiesById.Values;
    public IEnumerable<HassDevice> GetDevices() => _devicesById.Values;
    public IEnumerable<HassArea> GetAreas() => _areasById.Values;
    public IEnumerable<HassFloor> GetFloors() => _floorsById.Values;
    public IEnumerable<HassLabel> GetLabels() => _labelsById.Values;

    public HassEntity? GetHassEntityById(string? id) => id is null ? null : _entitiesById.GetValueOrDefault(id);
    public HassDevice? GetDeviceById(string? deviceId) => deviceId is null ? null : _devicesById.GetValueOrDefault(deviceId);
    public HassArea? GetAreaById(string? areaId) => areaId is null ? null : _areasById.GetValueOrDefault(areaId);
    public HassFloor? GetFloorById(string? floorId) => floorId is null ? null : _floorsById.GetValueOrDefault(floorId);
    public HassLabel? GetLabelById(string? labelId) => labelId is null ? null : _labelsById[labelId];


    public IEnumerable<HassEntity> GetEntitiesForArea(string? areaId) => _entitiesByAreaId[areaId];

    public IEnumerable<HassEntity> GetEntitiesForDevice(string? deviceId) => _entitiesByDeviceId[deviceId];

    public IEnumerable<HassEntity> GetEntitiesForLabel(string labelId) => _entitiesByLabel[labelId];

    public IEnumerable<HassDevice> GetDevicesForArea(string? areaId) => _devicesByAreaId[areaId];

    public IEnumerable<HassArea> GetAreasForFloor(string? floorId) => _areasByFloorId[floorId];


    private string? FindArea(HassEntity? entity)
    {
        if (!string.IsNullOrEmpty(entity?.AreaId))
        {
            return entity.AreaId;
        }

        return entity?.DeviceId is null ? null : _devicesById.GetValueOrDefault(entity.DeviceId!)?.AreaId;
    }

    public void Dispose()
    {
        _eventSubscription?.Dispose();
    }
}
