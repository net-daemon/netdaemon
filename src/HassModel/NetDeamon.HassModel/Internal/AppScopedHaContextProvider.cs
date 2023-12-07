using NetDaemon.Client.HomeAssistant.Extensions;
using NetDaemon.Infrastructure.ObservableHelpers;

namespace NetDaemon.HassModel.Internal;

/// <summary>
///     Implements IHaContext and IEventProvider to be used in a scope like a NetDaemon App
///     (We could eg. also have an implementation that does not deal with scopes or caching etc to be used in Console apps)
/// </summary>
[SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
internal class AppScopedHaContextProvider : IHaContext, IAsyncDisposable
{
    private readonly IHomeAssistantApiManager _apiManager;
    private readonly EntityAreaCache _entityAreaCache;
    private readonly EntityStateCache _entityStateCache;

    private readonly IHomeAssistantRunner _hassRunner;
    private readonly IQueuedObservable<HassEvent> _queuedObservable;
    private readonly IBackgroundTaskTracker _backgroundTaskTracker;

    private readonly CancellationTokenSource _tokenSource = new();

    public AppScopedHaContextProvider(
        EntityStateCache entityStateCache,
        EntityAreaCache entityAreaCache,
        IHomeAssistantRunner hassRunner,
        IHomeAssistantApiManager apiManager,
        IQueuedObservable<HassEvent> queuedObservable,
        IBackgroundTaskTracker backgroundTaskTracker
    )
    {
        _entityStateCache = entityStateCache;
        _entityAreaCache = entityAreaCache;
        _hassRunner = hassRunner;
        _apiManager = apiManager;

        // Create ScopedObservables for this app
        // This makes sure we will unsubscribe when this ContextProvider is Disposed
        _queuedObservable = queuedObservable;
        _backgroundTaskTracker = backgroundTaskTracker;
        _queuedObservable.Initialize(_entityStateCache.AllEvents);
    }

    public EntityState? GetState(string entityId)
    {
        return _entityStateCache.GetState(entityId).Map();
    }

    public Area? GetAreaFromEntityId(string entityId)
    {
        return _entityAreaCache.GetArea(entityId)?.Map();
    }

    public IReadOnlyList<Entity> GetAllEntities()
    {
        return _entityStateCache.AllEntityIds.Select(id => new Entity(this, id)).ToList();
    }

    public void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        _backgroundTaskTracker.TrackBackgroundTask(_hassRunner.CurrentConnection?.CallServiceAsync(domain, service, data, target.Map(), _tokenSource.Token), "Error in sending event");
    }

    public async Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        _ = _hassRunner.CurrentConnection ?? throw new InvalidOperationException("No connection to Home Assistant");
        var result = await _hassRunner.CurrentConnection
            .CallServiceWithResponseAsync(domain, service, data, target?.Map(), _tokenSource.Token)
            .ConfigureAwait(false);
       return result?.Response;
    }

    public IObservable<StateChange> StateAllChanges()
    {
        return _queuedObservable.Where(n =>
            n.EventType == "state_changed")
            .Select(n => n.ToStateChangedEvent()!)
            .Select(e => e.Map(this));
    }

    public IObservable<Event> Events => _queuedObservable
        .Select(e => e.Map());


    public void SendEvent(string eventType, object? data = null)
    {
        _backgroundTaskTracker.TrackBackgroundTask(_apiManager.SendEventAsync(eventType, _tokenSource.Token, data), "Error in sending event");
    }

    public async ValueTask DisposeAsync()
    {
        if (!_tokenSource.IsCancellationRequested)
            await _tokenSource.CancelAsync();
        _tokenSource.Dispose();
    }
}
