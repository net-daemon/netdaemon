using System.Collections.ObjectModel;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.Client.HomeAssistant.Extensions;

namespace NetDaemon.HassModel.Internal;

internal class RegistryCache(IHomeAssistantRunner hassRunner, ILogger<RegistryCache> logger) : IDisposable
{
    private CancellationToken _cancellationToken;
    private IDisposable? _eventSubscription;

    private IReadOnlyCollection<HassEntity> _entities = new Collection<HassEntity>();

    private Dictionary<string, HassDevice> _devicesById = new();
    private Dictionary<string, HassArea> _areasById = new ();
    private Dictionary<string, HassEntity> _entitesById = new ();

    private ILookup<string?, HassEntity> _entitiesByAreaId = Array.Empty<HassEntity>().ToLookup(e=>e.AreaId);
    private ILookup<string?, HassEntity> _entitiesByDeviceId = Array.Empty<HassEntity>().ToLookup(e=>e.DeviceId);
    private ILookup<string?, HassDevice> _devicesByAereaId = Array.Empty<HassDevice>().ToLookup(e=>e.AreaId);
    private ILookup<string?, HassLabel> _labelsByEntityId = Array.Empty<HassLabel>().ToLookup(_=>default(string));

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

        var areas = await hassRunner.CurrentConnection.GetAreasAsync(_cancellationToken).ConfigureAwait(false);
        _areasById = areas?.ToDictionary(k => k.Id!, v => v) ?? new Dictionary<string, HassArea>();

        _entitiesByAreaId = _entities.ToLookup(FindArea);
        _entitiesByDeviceId = _entities.ToLookup(e => e.DeviceId);
        _devicesByAereaId = devices.ToLookup(d => d.AreaId);

        // TODO: load labels
         _labelsByEntityId = Array.Empty<HassLabel>().ToLookup(e=>default(string));
    }

    public HassEntity? GetHassEntityById(string id) => _entitesById.GetValueOrDefault(id, null!);

    public IEnumerable<HassEntity> GetEntitiesForArea(string? areaId) => _entitiesByAreaId[areaId];

    public IEnumerable<HassEntity> GetEntitiesForDevice(string? deviceId) => _entitiesByDeviceId[deviceId];

    public IEnumerable<HassDevice> GetDevicesForArea(string? areaId) => _devicesByAereaId[areaId];

    public HassDevice? GetDeviceById(string? deviceId) =>
        deviceId is null ? null : _devicesById.GetValueOrDefault(deviceId, null!);

    public HassArea? GetAreaById(string? areaId) =>
        areaId is null ? null : _areasById.GetValueOrDefault(areaId, null!);

    public HassArea? GetAreaForEntity(HassEntity? entity) => GetAreaById(FindArea(entity));

    public IEnumerable<HassLabel> GetLabelsForEntity(string? entityId)
    {
        return (entityId is null ? null : _entitesById.GetValueOrDefault(entityId)?.Labels.Select(l => new HassLabel(l, l, null, "label for " + l, null) { Name = l, LabelId = l}))
                ?? Array.Empty<HassLabel>();
    }

    public void Dispose()
    {
        _eventSubscription?.Dispose();
    }

    private string? FindArea(HassEntity? entity)
    {
        if (!string.IsNullOrEmpty(entity?.AreaId))
        {
            return entity.AreaId;
        }

        return entity?.DeviceId is null ? null : _devicesById.GetValueOrDefault(entity?.DeviceId!)?.AreaId;
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

    public IEnumerable<HassEntity> GetEntitiesForLabel(string labelId)
    {
        return _entities.Where(e => e.Labels.Any(l => l == labelId));
    }
}
