using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.Model3.Internal
{
    [SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
    internal class EntityStateCache : IDisposable
    {
        private readonly IHassClient _hassClient;
        private readonly Subject<HassStateChangedEventData> _innerSubject = new();
        private readonly IDisposable _eventSubscription;
        private readonly ConcurrentDictionary<string, HassState?> _latestStates = new();

        public EntityStateCache(IHassClient hassClient, IObservable<HassEvent> events)
        {
            _hassClient = hassClient;

            _eventSubscription = events.Subscribe(HandleEvent);
        }

        private void HandleEvent(HassEvent hassEvent)
        {
            if (hassEvent.Data is not HassStateChangedEventData hassStateChangedEventData) return;
            
            // Make sure to first add the new state to the cache before calling other subscribers.
            _latestStates[hassStateChangedEventData.EntityId] = hassStateChangedEventData.NewState;

            _innerSubject.OnNext(hassStateChangedEventData);
        }

        public IObservable<HassStateChangedEventData> StateAllChanges => _innerSubject;

        public HassState? GetState(string entityId)
        {
            // Load missing states on demand,
            // this is a blocking call if it is not present but we want to avoid making GetState async

            return _latestStates.GetOrAdd(entityId, _ => _hassClient.GetState(entityId).Result);
        }

        public void Dispose()
        {
            _innerSubject.Dispose();
            _eventSubscription.Dispose();
        }
    }
}