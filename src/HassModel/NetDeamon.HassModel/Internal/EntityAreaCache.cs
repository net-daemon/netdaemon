using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.HassModel.Internal;

internal class EntityAreaCache : IDisposable
{
    private bool _initialized;
    private readonly IDisposable _eventSubscription;
    private readonly IHassClient _hassClient;
    private Dictionary<string, HassArea> _latestAreas = new();

    public EntityAreaCache(IHassClient hassClient, IObservable<HassEvent> events)
    {
        _hassClient = hassClient;

        _eventSubscription = events.Subscribe(HandleEvent);
    }

    public async Task InitializeAsync()
    {
        await LoadAreas().ConfigureAwait(false);

        _initialized = true;
    }

    public HassArea? GetArea(string entityId)
    {
        if (!_initialized) throw new InvalidOperationException("AreaCache has not been initialized yet");

        return _latestAreas.TryGetValue(entityId, out var result) ? result : null;
    }

    public void Dispose()
    {
        _eventSubscription.Dispose();
    }

    private async Task LoadAreas()
    {
        var entities = await _hassClient.GetEntities().ConfigureAwait(false);
        var devices = await _hassClient.GetDevices().ConfigureAwait(false);
        var deviceDict = devices?.ToDictionary(k => k.Id!, v => v);
        var areas = await _hassClient.GetAreas().ConfigureAwait(false);
        var areaDict = areas?.ToDictionary(k => k.Id!, v => v);

        if (deviceDict is null || areaDict is null)
        {
            return;
        }

        var latestAreas = new Dictionary<string, HassArea>();
        
        foreach (var entity in entities)
        {
            if (!string.IsNullOrEmpty(entity.AreaId) && areaDict.TryGetValue(entity.AreaId, out var hassArea))
            {
                latestAreas[entity.EntityId!] = hassArea;
            }
            else if (!string.IsNullOrEmpty(entity.DeviceId)
                     && deviceDict.TryGetValue(entity.DeviceId, out var device)
                     && !string.IsNullOrEmpty(device.AreaId)
                     && areaDict.TryGetValue(device.AreaId, out hassArea))
            {
                latestAreas[entity.EntityId!] = hassArea;
            }
        }

        _latestAreas = latestAreas;
    }

    private void HandleEvent(HassEvent hassEvent)
    {
        if (hassEvent.EventType is "device_registry_updated" or "area_registry_updated")
        {
            // Fire and forget
            _ = LoadAreas().ConfigureAwait(false);
        }
    }
}