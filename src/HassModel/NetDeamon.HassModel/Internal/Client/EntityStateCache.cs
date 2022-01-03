using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Client.Common;
using NetDaemon.Client.Common.HomeAssistant.Extensions;
using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.HassModel.Internal.Client;

[SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
internal class EntityStateCache : IDisposable
{
    private readonly IDisposable _eventSubscription;
    private readonly IHomeAssistantRunner _hassRunner;
    private readonly Subject<HassStateChangedEventData> _innerSubject = new();
    private readonly ConcurrentDictionary<string, HassState?> _latestStates = new();

    private bool _initialized;

    public EntityStateCache(IHomeAssistantRunner hassRunner, IObservable<HassEvent> events)
    {
        _hassRunner = hassRunner;

        _eventSubscription = events.Subscribe(HandleEvent);
    }

    public IEnumerable<string> AllEntityIds => _latestStates.Select(s => s.Key);

    public IObservable<HassStateChangedEventData> StateAllChanges => _innerSubject;

    public void Dispose()
    {
        _innerSubject.Dispose();
        _eventSubscription.Dispose();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _ = _hassRunner.CurrentConnection ?? throw new InvalidOperationException();

        var hassStates = await _hassRunner.CurrentConnection.GetStatesAsync(cancellationToken).ConfigureAwait(false);

        if (hassStates is not null)
            foreach (var hassClientState in hassStates)
                _latestStates[hassClientState.EntityId] = hassClientState;
        _initialized = true;
    }

    private void HandleEvent(HassEvent hassEvent)
    {
        if (hassEvent.EventType != "state_changed") return;

        var hassStateChangedEventData = hassEvent.ToStateChangedEvent()
                                        ?? throw new InvalidOperationException(
                                            "Error when parsing state changed event");

        // Make sure to first add the new state to the cache before calling other subscribers.
        _latestStates[hassStateChangedEventData.EntityId] = hassStateChangedEventData.NewState;
        _innerSubject.OnNext(hassStateChangedEventData);
    }

    public HassState? GetState(string entityId)
    {
        if (!_initialized) throw new InvalidOperationException("StateCache has not been initialized yet");

        return _latestStates.TryGetValue(entityId, out var result) ? result : null;
    }
}