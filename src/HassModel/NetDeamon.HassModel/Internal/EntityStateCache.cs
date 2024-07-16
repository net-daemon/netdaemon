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
    private readonly ConcurrentDictionary<string, Lazy<EntityState?>> _latestStates = new();

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
                _latestStates[hassClientState.EntityId] = new Lazy<EntityState?>(()=>hassClientState.Map());
        _initialized = true;
    }

    private void HandleEvent(HassEvent hassEvent)
    {
        if (hassEvent.EventType == "state_changed")
        {
            // Make sure to first add the new state to the cache before calling other subscribers.
            var entityId = hassEvent.DataElement?.GetProperty("entity_id").GetString()!;

            _latestStates[entityId] = new Lazy<EntityState?>(() => hassEvent.DataElement?.GetProperty("new_state").Deserialize<HassState>().Map());
        };
        _eventSubject.OnNext(hassEvent);
    }

    public EntityState? GetState(string entityId)
    {
        if (!_initialized) throw new InvalidOperationException("StateCache has not been initialized yet");

        return _latestStates.GetValueOrDefault(entityId)?.Value;
    }
}
