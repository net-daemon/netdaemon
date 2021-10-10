using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Model3.Entities;

namespace NetDaemon.Model3.Internal
{
    [SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
    internal class EntityStateCache : IDisposable
    {
        private readonly IHassClient _hassClient;
        private readonly Subject<HassStateChangedEventData> _innerSubject = new();
        private readonly IDisposable _eventSubscription;
        private readonly ConcurrentDictionary<string, HassState?> _latestStates = new();

        private bool _initialized;

        public EntityStateCache(IHassClient hassClient, IObservable<HassEvent> events)
        {
            _hassClient = hassClient;

            _eventSubscription = events.Subscribe(HandleEvent);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            var hassStates = await _hassClient.GetAllStates(cancellationToken).ConfigureAwait(false);
            
            foreach (var hassClientState in hassStates)
            {
                _latestStates[hassClientState.EntityId] = hassClientState;
            }
            _initialized = true;
        }

        public IEnumerable<string> AllEntityIds => _latestStates.Select(s => s.Key);

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
            if (!_initialized) throw new InvalidOperationException("StateCache has not been initialized yet");

            return _latestStates.TryGetValue(entityId, out var result) ? result : null;
        }

        public void Dispose()
        {
            _innerSubject.Dispose();
            _eventSubscription.Dispose();
        }
    }
}