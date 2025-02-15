using System.Reactive.Concurrency;
using NetDaemon.Client.HomeAssistant.Extensions;

namespace NetDaemon.HassModel.Internal;

internal class RegistryCache(IHomeAssistantRunner hassRunner, ILogger<RegistryCache> logger) : IDisposable
{
    private CancellationToken _cancellationToken;
    private readonly List<IDisposable> _toDispose = [];

    // Just in case this cache is used before it is initialized we will initialize all
    // these dictionaries and lookups empty to avoid exceptions and not having to make these nullable
    private Dictionary<string, HassLabel> _labelsById = [];
    private Dictionary<string, HassFloor> _floorsById = [];
    private Dictionary<string, HassArea> _areasById = [];
    private Dictionary<string, HassDevice> _devicesById = [];
    private Dictionary<string, HassEntity> _entitiesById = [];

    private ILookup<string?, HassEntity> _entitiesByAreaId = Array.Empty<HassEntity>().ToLookup(e=>e.AreaId);
    private ILookup<string?, HassEntity> _entitiesByDeviceId = Array.Empty<HassEntity>().ToLookup(e=>e.DeviceId);
    private ILookup<string?, HassDevice> _devicesByAreaId = Array.Empty<HassDevice>().ToLookup(e=>e.AreaId);
    private ILookup<string?, HassArea> _areasByFloorId = Array.Empty<HassArea>().ToLookup(e => e.FloorId);
    private ILookup<string, HassEntity> _entitiesByLabel = Array.Empty<HassEntity>().ToLookup(e =>default(string)!);

    // We use this connection here during startup, and after we have received .._registry_updates events. In both cases we expect the connection to be available
    private IHomeAssistantConnection CurrentConnection => hassRunner.CurrentConnection ?? throw new InvalidOperationException("Home assistantConnection is not available ");

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        await SubscribeRegistryUpdates().ConfigureAwait(false);

        logger.LogInformation("Initializing RegistryCache");

        await ReloadEntities();
        await ReloadDevices();
        await ReloadAreas();
        await ReloadLabels();
        await ReloadFloors();
        UpdateEntitiesByAreaId();
    }

    private async Task SubscribeRegistryUpdates()
    {
        var events = await CurrentConnection.SubscribeToHomeAssistantEventsAsync(null, _cancellationToken).ConfigureAwait(false);

        // each registry has its own registry_updated event that will be send by HA when it is updated
        SubscribeRegistryUpdate(events, "entity_registry_updated", async ()=> { await ReloadEntities(); UpdateEntitiesByAreaId(); });
        SubscribeRegistryUpdate(events, "device_registry_updated", async ()=> { await ReloadDevices(); UpdateEntitiesByAreaId(); });
        SubscribeRegistryUpdate(events, "area_registry_updated", ReloadAreas);
        SubscribeRegistryUpdate(events, "label_registry_updated", ReloadLabels);
        SubscribeRegistryUpdate(events, "floor_registry_updated", ReloadFloors);
    }

    private void SubscribeRegistryUpdate(IObservable<HassEvent> events, string eventType, Func<Task> handler)
    {
        _toDispose.Add(events
            .Where(e => e.EventType == eventType)
            // On some systems we found the registry_updated events are send frequently without actual user action in HA,
            // therefore we throttle updating the cache.
            .ThrottleAfterFirstEvent(TimeSpan.FromMinutes(5), Scheduler.Default)
            .SubscribeAsync(async _ =>
                {
                    logger.LogDebug("Received {Event}: Updating RegistryCache", eventType);

                    await handler();
                },
                ex => logger.LogError(ex, "Exception updating RegistryCache")));
    }

    private async Task ReloadEntities()
    {
        var entities = await CurrentConnection.GetEntitiesAsync(_cancellationToken).ConfigureAwait(false) ?? [];
        _entitiesById = SafeToDictionaryById(entities, e => e.EntityId);
        _entitiesByLabel = entities.SelectMany(e => e.Labels.Select(l => (e, l))).ToLookup(t => t.l, t => t.e);
        _entitiesByDeviceId = entities.ToLookup(e => e.DeviceId);
    }

    private async Task ReloadDevices()
    {
        var devices = await CurrentConnection.GetDevicesAsync(_cancellationToken).ConfigureAwait(false) ?? [];
        _devicesById = SafeToDictionaryById(devices, k => k.Id);
        _devicesByAreaId = devices.ToLookup(d => d.AreaId);
    }


    private async Task ReloadAreas()
    {
        var areas = await CurrentConnection.GetAreasAsync(_cancellationToken).ConfigureAwait(false) ?? [];
        _areasById = SafeToDictionaryById(areas, k => k.Id);
        _areasByFloorId = areas.ToLookup(a => a.FloorId);
    }

    private async Task ReloadFloors()
    {
        var floors = await CurrentConnection.GetFloorsAsync(_cancellationToken).ConfigureAwait(false) ?? [];
        _floorsById = SafeToDictionaryById(floors, f =>f.Id);
    }

    private async Task ReloadLabels()
    {
        var labels = await CurrentConnection.GetLabelsAsync(_cancellationToken).ConfigureAwait(false) ?? [];
        _labelsById = SafeToDictionaryById(labels, l => l.Id);
    }

    private void UpdateEntitiesByAreaId()
    {
        _entitiesByAreaId = _entitiesById.Values.ToLookup(FindArea);
    }

    private static Dictionary<string, T> SafeToDictionaryById<T>(IEnumerable<T> input, Func<T, string?> keySelector)
    {
        return input.Select(item => (item, key: keySelector(item))).Where(t => t.key is not null)
            .DistinctBy(t => t.item).ToDictionary(t => t.key!, t => t.item);
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
        // Usually an entity gets its area via the device, but it can be overriden at the entity level so we
        // first check if the entity has an area and then if it has a device which has an area
        return !string.IsNullOrEmpty(entity?.AreaId) ? entity.AreaId : GetDeviceById(entity?.DeviceId)?.AreaId;
    }

    public void Dispose()
    {
        _toDispose.ForEach(d => d.Dispose());
    }
}
