﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NetDaemon.Client.Common;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.Client.Common.HomeAssistant.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;
using NetDaemon.Infrastructure.ObservableHelpers;
using System.Threading;

namespace NetDaemon.HassModel.Internal.Client
{
    /// <summary>
    /// Implements IHaContext and IEventProvider to be used in a scope like a NetDaemon App
    /// (We could eg. also have an implementation that does not deal with scopes or caching etc to be used en Console apps)
    /// </summary>
    [SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
    internal class AppScopedHaContextProvider : IHaContext, IDisposable
    {
        private readonly EntityStateCache _entityStateCache;
        private readonly EntityAreaCache _entityAreaCache;

        private readonly IHomeAssistantRunner _hassRunner;
        private readonly IHomeAssistantApiManager _apiManager;
        private readonly ScopedObservable<HassEvent> _scopedEventObservable;
        private readonly ScopedObservable<HassStateChangedEventData> _scopedStateObservable;

        private CancellationTokenSource _tokenSource = new();

        public AppScopedHaContextProvider(IObservable<HassEvent> hassEventObservable,
            EntityStateCache entityStateCache,
            EntityAreaCache entityAreaCache,
            IHomeAssistantRunner hassRunner,
            IHomeAssistantApiManager apiManager)
        {
            _entityStateCache = entityStateCache;
            _entityAreaCache = entityAreaCache;
            _hassRunner = hassRunner;
            _apiManager = apiManager;

            // Create ScopedObservables for this app
            // This makes sure we will unsubscribe when this ContextProvider is Disposed
            _scopedEventObservable = new ScopedObservable<HassEvent>(hassEventObservable);
            _scopedStateObservable = new ScopedObservable<HassStateChangedEventData>(_entityStateCache.StateAllChanges);
        }

        public EntityState? GetState(string entityId) => _entityStateCache.GetState(entityId).Map();

        public Area? GetAreaFromEntityId(string entityId) => _entityAreaCache.GetArea(entityId)?.Map();

        public IReadOnlyList<Entity> GetAllEntities() => _entityStateCache.AllEntityIds.Select(id => new Entity(this, id)).ToList();

        public void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
        {
            _hassRunner.CurrentConnection?.CallServiceAsync(domain, service, data, target.Map(), _tokenSource.Token);
        }

        public IObservable<StateChange> StateAllChanges() => _scopedStateObservable.Select(e => e.Map(this));

        public IObservable<Event> Events => _scopedEventObservable.Select(e => e.Map());

        public void SendEvent(string eventType, object? data = null) =>
            // For now we do just a fire and forget of the async SendEvent method. HassClient will handle and log exceptions 
            _apiManager.SendEventAsync(eventType, _tokenSource.Token, data);

        public void Dispose()
        {
            _scopedEventObservable.Dispose();
            _scopedStateObservable.Dispose();
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
