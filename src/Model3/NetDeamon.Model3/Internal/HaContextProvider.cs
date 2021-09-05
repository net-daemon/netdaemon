using System;
using System.Reactive.Linq;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using System.Diagnostics.CodeAnalysis;
using NetDaemon.Infrastructure.ObservableHelpers;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;

namespace NetDaemon.Model3.Internal
{
    [SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
    internal class HaContextProvider : IHaContext, IDisposable, IEventProvider
    {
        private readonly EntityStateCache _entityStateCache;
        private readonly IHassClient _hassClient;
        private readonly ScopedObservable<HassEvent> _scopedEventObservable;
        private readonly ScopedObservable<HassStateChangedEventData> _scopedStateObservable;

        public HaContextProvider(IObservable<HassEvent> hassEventObservable,
            EntityStateCache entityStateCache,
            IHassClient hassClient)
        {
            _entityStateCache = entityStateCache;
            _hassClient = hassClient;

            // Create ScopedObservables for this app
            // This makes sure we will unsubscribe when this ContextProvider is Disposed
            _scopedEventObservable = new ScopedObservable<HassEvent>(hassEventObservable);
            _scopedStateObservable = new ScopedObservable<HassStateChangedEventData>(_entityStateCache.StateAllChanges);
        }

        public EntityState? GetState(string entityId)
        {
            var hassState = _entityStateCache.GetState(entityId);

            return HassObjectMapper.Map(hassState);
        }

        public void CallService(string domain, string service, object? data, Entity entity)
        {
            _hassClient.CallService(domain, service, data, new HassTarget { EntityIds = new[] { entity.EntityId } });
        }

        public IObservable<StateChange> StateAllChanges => _entityStateCache.StateAllChanges.Select(e => e.Map(this));

        public IObservable<StateChange> StateChanges => StateAllChanges.Where(e => e.New?.State != e.Old?.State);

        public IObservable<T> GetEventDataOfType<T>(string eventType) where T : class => 
            _scopedEventObservable
                .Where(e => e.EventType == eventType && e.DataElement != null)
                .Select(e => e.DataElement?.ToObject<T>()!);

        public void Dispose()
        {
            _scopedEventObservable.Dispose();
            _scopedStateObservable.Dispose();
        }
    }
}