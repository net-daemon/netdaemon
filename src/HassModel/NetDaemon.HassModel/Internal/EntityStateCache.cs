using System.Collections.Concurrent;
using NetDaemon.Client.HomeAssistant.Extensions;

namespace NetDaemon.HassModel.Internal;

internal class EntityStateCache(IHomeAssistantRunner hassRunner) : IDisposable
{
    private IDisposable? _eventSubscription;
    private readonly Subject<HassEvent> _eventSubject = new();
    private readonly ConcurrentDictionary<string, Lazy<EntityState?>> _latestStates = new();

    private bool _initialized;

    public IEnumerable<string> AllEntityIds => _latestStates.Select(s => s.Key);

    public IObservable<HassEvent> AllEvents => _eventSubject;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _ = hassRunner.CurrentConnection ?? throw new InvalidOperationException();

        var events = await hassRunner.CurrentConnection!.SubscribeToHomeAssistantEventsAsync(null,  cancellationToken).ConfigureAwait(false);
        _eventSubscription = events.Subscribe(HandleEvent);

        var hassStates = await hassRunner.CurrentConnection.GetStatesAsync(cancellationToken).ConfigureAwait(false);

        foreach (var hassClientState in hassStates ?? [])
        {
            _latestStates[hassClientState.EntityId] = new Lazy<EntityState?>(hassClientState.Map);
        }

        _initialized = true;
    }

    private void HandleEvent(HassEvent hassEvent)
    {
        // This method is in the 'Hot Path' as it gets executed for every event HA sends even if we are not really interested in it
        if (hassEvent.EventType == "state_changed")
        {
            var entityId = hassEvent.DataElement?.GetProperty("entity_id").GetString()!;
            var newStateElement = hassEvent.DataElement?.GetProperty("new_state");

            // We want to avoid deserializing to a EntityState if not needed as that is an expensive part,
            // so we cache a Lazy instead that will deserialize only if needed
            _latestStates[entityId] = new Lazy<EntityState?>(() => newStateElement?.Deserialize<EntityState>());
        }

        // Make sure we call other subscribers after we added the new state to the cache, so observers can also see the new value in the cache
        _eventSubject.OnNext(hassEvent);
    }

    public EntityState? GetState(string entityId)
    {
        return !_initialized
            ? throw new InvalidOperationException(
                "StateCache has not been initialized. This may occur if NetDaemon has not yet completed its initial connection to Home Assistant. " +
                "This should not happen with a standard NetDaemon deployment. If you're using a custom deployment, ensure that initialization is complete " +
                "before accessing entity state by awaiting `WaitForInitializationAsync` on `INetDaemonRuntime`."
            )
            : _latestStates.GetValueOrDefault(entityId)?.Value;
    }

    public void Dispose()
    {
        _eventSubscription?.Dispose();
        _eventSubject.Dispose();
    }
}
