using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;

namespace NetDaemon.Model3.Internal
{
    [SuppressMessage("", "CA1812", Justification = "Is Loaded via DependencyInjection")]
    internal class HaContextProvider : IHaContext
    {
        private readonly IObservable<HassEvent> _hassEventObservable;
        private readonly EntityStateCache _entityStateCache;
        private readonly IHassClient _hassClient;

        public HaContextProvider(
            IObservable<HassEvent> hassEventObservable,
            EntityStateCache entityStateCache,
            IHassClient hassClient)
        {
            _hassEventObservable = hassEventObservable;
            _entityStateCache = entityStateCache;
            _hassClient = hassClient;

            var stateChanges = _hassEventObservable
                .Select(e => e.Data as HassStateChangedEventData)
                .Where(e => e != null)
                .Select(e => e!);

            stateChanges.Subscribe(s =>
            {
                var state = s.NewState;
                if (state != null) _entityStateCache.Store(state);
            });

            StateAllChanges = stateChanges.Select(e => e.Map(this));
        }

        public EntityState? GetState(string entityId)
        {
            var hassState = _entityStateCache.GetState(entityId);

            return HassObjectMapper.Map(hassState);
        }

        public void CallService(string domain, string service, object? data, Entity entity)
        {
            _hassClient.CallService(domain, service,
                data,
                new HassTarget { EntityIds = new[] { entity.EntityId } });
        }

        //public IRxEvent EventChanges { get; }
        public IObservable<StateChange> StateAllChanges { get; }

        public IObservable<StateChange> StateChanges => StateAllChanges.Where(e => e.New?.State != e.Old?.State);
    }
}