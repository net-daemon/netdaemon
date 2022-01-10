using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Client.Common;
using NetDaemon.Client.Common.HomeAssistant.Extensions;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using NetDaemon.Infrastructure.ObservableHelpers;

namespace NetDaemon.HassModel.Internal.Client;

/// <summary>
///     Implements IHaContext and IEventProvider to be used in a scope like a NetDaemon App
///     (We could eg. also have an implementation that does not deal with scopes or caching etc to be used en Console apps)
/// </summary>
[SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
internal class AppScopedHaContextProvider : IHaContext, IAsyncDisposable
{
    private readonly IHomeAssistantApiManager _apiManager;
    private readonly EntityAreaCache _entityAreaCache;
    private readonly EntityStateCache _entityStateCache;

    private readonly IHomeAssistantRunner _hassRunner;
    private readonly IQueuedObservable<HassEvent> _queuedObservable;

    private readonly CancellationTokenSource _tokenSource = new();

    public AppScopedHaContextProvider(
        EntityStateCache entityStateCache,
        EntityAreaCache entityAreaCache,
        IHomeAssistantRunner hassRunner,
        IHomeAssistantApiManager apiManager,
        IQueuedObservable<HassEvent> queuedObservable
        )
    {
        _entityStateCache = entityStateCache;
        _entityAreaCache = entityAreaCache;
        _hassRunner = hassRunner;
        _apiManager = apiManager;

        // Create ScopedObservables for this app
        // This makes sure we will unsubscribe when this ContextProvider is Disposed
        _queuedObservable = queuedObservable;
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
        _hassRunner.CurrentConnection?.CallServiceAsync(domain, service, data, target.Map(), _tokenSource.Token);
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
        // For now we do just a fire and forget of the async SendEvent method. HassClient will handle and log exceptions 
        _apiManager.SendEventAsync(eventType, _tokenSource.Token, data);
    }

    public async ValueTask DisposeAsync()
    {
        await _queuedObservable.DisposeAsync().ConfigureAwait(false);
        if (!_tokenSource.IsCancellationRequested)
            _tokenSource.Cancel();
        _tokenSource.Dispose();
    }
}