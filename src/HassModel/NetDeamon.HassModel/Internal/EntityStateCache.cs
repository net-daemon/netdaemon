using System.Collections.Concurrent;
using NetDaemon.Client.HomeAssistant.Extensions;

namespace NetDaemon.HassModel.Internal;

[SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
internal class EntityStateCache : IDisposable
{
    private IDisposable? _eventSubscription;
    private readonly IHomeAssistantRunner _hassRunner;
    private readonly IServiceProvider _provider;
    private readonly Subject<HassEvent> _eventSubject = new();
    private readonly Subject<HassStateChangedEventData> _innerSubject = new();
    private readonly ConcurrentDictionary<string, HassState?> _latestStates = new();

    private bool _initialized;

    public EntityStateCache(IHomeAssistantRunner hassRunner, IServiceProvider provider)
    {
        _hassRunner = hassRunner;
        _provider = provider;
    }

    public IEnumerable<string> AllEntityIds => _latestStates.Select(s => s.Key);

    public IObservable<HassEvent> AllEvents => _eventSubject;

    public void Dispose()
    {
        _innerSubject.Dispose();
        _eventSubscription?.Dispose();
        _eventSubject.Dispose();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _ = _hassRunner.CurrentConnection ?? throw new InvalidOperationException();

        var events = await _hassRunner.CurrentConnection!.SubscribeToHomeAssistantEventsAsync(null,  cancellationToken).ConfigureAwait(false);
        _eventSubscription = events.Subscribe(HandleEvent);

        var hassStates = await _hassRunner.CurrentConnection.GetStatesAsync(cancellationToken).ConfigureAwait(false);

        if (hassStates is not null)
            foreach (var hassClientState in hassStates)
                _latestStates[hassClientState.EntityId] = hassClientState;
        _initialized = true;
    }

    private void HandleEvent(HassEvent hassEvent)
    {
        if (hassEvent.EventType == "state_changed")
        {
            var hassStateChangedEventData = hassEvent.ToStateChangedEvent()
                                            ?? throw new InvalidOperationException(
                                                "Error when parsing state changed event");

            // Make sure to first add the new state to the cache before calling other subscribers.
            _latestStates[hassStateChangedEventData.EntityId] = hassStateChangedEventData.NewState;
        };
        _eventSubject.OnNext(hassEvent);
    }

    public HassState? GetState(string entityId)
    {
        if (!_initialized) throw new InvalidOperationException("StateCache has not been initialized yet");

        return _latestStates.TryGetValue(entityId, out var result) ? result : null;
    }
}