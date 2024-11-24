using NetDaemon.Client.HomeAssistant.Extensions;

namespace NetDaemon.HassModel.Internal;

/// <summary>
///     Implements IHaContext and IEventProvider to be used in a scope like a NetDaemon App
///     (We could eg. also have an implementation that does not deal with scopes or caching etc to be used in Console apps)
/// </summary>
[SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
internal class AppScopedHaContextProvider : IHaContext, IAsyncDisposable
{
    private volatile bool _isDisposed;
    private volatile bool _isDisposing;
    private readonly IHomeAssistantApiManager _apiManager;
    private readonly EntityStateCache _entityStateCache;

    private readonly IHomeAssistantRunner _hassRunner;
    private readonly QueuedObservable<HassEvent> _queuedObservable;
    private readonly IBackgroundTaskTracker _backgroundTaskTracker;
    private readonly IEntityFactory _entityFactory;

    private readonly CancellationTokenSource _tokenSource = new();

    public AppScopedHaContextProvider(
        EntityStateCache entityStateCache,
        IHomeAssistantRunner hassRunner,
        IHomeAssistantApiManager apiManager,
        IBackgroundTaskTracker backgroundTaskTracker,
        IServiceProvider serviceProvider,
        ILogger<AppScopedHaContextProvider> logger,
        IEntityFactory entityFactory)
    {
        _entityStateCache = entityStateCache;
        _hassRunner = hassRunner;
        _apiManager = apiManager;

        // Create QueuedObservable for this app
        // This makes sure we will unsubscribe when this ContextProvider is Disposed
        _queuedObservable = new QueuedObservable<HassEvent>(_entityStateCache.AllEvents, logger);
        _backgroundTaskTracker = backgroundTaskTracker;
        _entityFactory = entityFactory;

        // The HaRegistry needs a reference to this AppScopedHaContextProvider And we need the reference
        // to the AppScopedHaContextProvider here. Therefore we create it manually providing this
        Registry = ActivatorUtilities.CreateInstance<HaRegistry>(serviceProvider, this);
    }

    // By making the HaRegistry instance internal it can also be registered as scoped in the DI container and injected into applications
    internal HaRegistry Registry { get; }

    public EntityState? GetState(string entityId)
    {
        return _entityStateCache.GetState(entityId);
    }

    [Obsolete("Use Registry to navigate Entities, Devices and Areas")]
    public Area? GetAreaFromEntityId(string entityId)
    {
        return GetEntityRegistration(entityId)?.Area;
    }

    public EntityRegistration? GetEntityRegistration(string entityId) => Registry.GetEntityRegistration(entityId);

    public IReadOnlyList<Entity> GetAllEntities()
    {
        return _entityStateCache.AllEntityIds.Select(id => _entityFactory.CreateEntity(this, id)).ToList();
    }

    public Entity Entity(string entityId) => _entityFactory.CreateEntity(this, entityId);


    public void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _ = _hassRunner.CurrentConnection ?? throw new InvalidOperationException("No connection to Home Assistant");

        _backgroundTaskTracker.TrackBackgroundTask(_hassRunner.CurrentConnection.CallServiceAsync(domain, service, data, target.Map(), _tokenSource.Token), "Error in sending event");
    }

    public async Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _ = _hassRunner.CurrentConnection ?? throw new InvalidOperationException("No connection to Home Assistant");

        var result = await _hassRunner.CurrentConnection
            .CallServiceWithResponseAsync(domain, service, data, target?.Map(), _tokenSource.Token)
            .ConfigureAwait(false);
       return result?.Response;
    }

    public IObservable<StateChange> StateAllChanges()
    {
        return _queuedObservable
            .Where(n => n.EventType == "state_changed")
            .Select(n => new StateChange(n.DataElement.GetValueOrDefault(), this));
    }

    public IObservable<Event> Events => _queuedObservable
        .Select(e => e.Map());

    public void SendEvent(string eventType, object? data = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _backgroundTaskTracker.TrackBackgroundTask(_apiManager.SendEventAsync(eventType, _tokenSource.Token, data), "Error in sending event");
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposing) return;
        _isDisposing = true;

        // The order here is important, we want to allow apps to process their pending events and wait for background tasks to complete
        // before actually shutting down
        await _queuedObservable.DisposeAsync().ConfigureAwait(false);
        await _backgroundTaskTracker.DisposeAsync().ConfigureAwait(false);

        await _tokenSource.CancelAsync().ConfigureAwait(false);

        _isDisposed = true;
        _tokenSource.Dispose();
    }
}
