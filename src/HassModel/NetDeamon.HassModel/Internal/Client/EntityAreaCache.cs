using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Client.Common;
using NetDaemon.Client.Common.HomeAssistant.Extensions;
using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.HassModel.Internal.Client;

internal class EntityAreaCache : IDisposable
{
    private readonly IDisposable _eventSubscription;
    private readonly IHomeAssistantRunner _hassRunner;

    private CancellationToken _cancellationToken;
    private bool _initialized;
    private Dictionary<string, HassArea> _latestAreas = new();

    public EntityAreaCache(IHomeAssistantRunner hassRunner, IObservable<HassEvent> events)
    {
        _hassRunner = hassRunner;

        _eventSubscription = events.Subscribe(HandleEvent);
    }

    public void Dispose()
    {
        _eventSubscription.Dispose();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        await LoadAreas().ConfigureAwait(false);

        _initialized = true;
    }

    public HassArea? GetArea(string entityId)
    {
        if (!_initialized) throw new InvalidOperationException("AreaCache has not been initialized yet");

        return _latestAreas.TryGetValue(entityId, out var result) ? result : null;
    }

    private async Task LoadAreas()
    {
        _ = _hassRunner?.CurrentConnection ?? throw new InvalidOperationException();

        var entities = await _hassRunner.CurrentConnection.GetEntitiesAsync(_cancellationToken).ConfigureAwait(false);
        var devices = await _hassRunner.CurrentConnection.GetDevicesAsync(_cancellationToken).ConfigureAwait(false);
        var deviceDict = devices?.ToDictionary(k => k.Id!, v => v);
        var areas = await _hassRunner.CurrentConnection.GetAreasAsync(_cancellationToken).ConfigureAwait(false);
        var areaDict = areas?.ToDictionary(k => k.Id!, v => v);

        if (deviceDict is null || areaDict is null) return;

        var latestAreas = new Dictionary<string, HassArea>();

        if (entities is not null)
            foreach (var entity in entities)
                if (!string.IsNullOrEmpty(entity.AreaId) && areaDict.TryGetValue(entity.AreaId, out var hassArea))
                    latestAreas[entity.EntityId!] = hassArea;
                else if (!string.IsNullOrEmpty(entity.DeviceId)
                         && deviceDict.TryGetValue(entity.DeviceId, out var device)
                         && !string.IsNullOrEmpty(device.AreaId)
                         && areaDict.TryGetValue(device.AreaId, out hassArea))
                    latestAreas[entity.EntityId!] = hassArea;
        _latestAreas = latestAreas;
    }

    private void HandleEvent(HassEvent hassEvent)
    {
        if (hassEvent.EventType is "device_registry_updated" or "area_registry_updated")
            // Fire and forget
            _ = LoadAreas().ConfigureAwait(false);
    }
}