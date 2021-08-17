using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Model3.ModelV3;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon;
using NetDaemon.Mapping;

namespace NetDaemon.Common.ModelV3
{
    internal class HaContextProvider : IHaContext
    {
        private readonly NetDaemonHost _netDaemonHost;
        private readonly ObservableBase<StateChange> _observable;
        
        // TODO: Initialize the stateCache here, would need some sort of init event, and access to the IHassClient 
        private readonly EntityStateCache _entityStateCache = new ();
        public HaContextProvider(NetDaemonHost netDaemonHost)
        {
            _netDaemonHost = netDaemonHost;
            _netDaemonHost.HassEvents +=NetDaemonHostOnHassEvents;
            
            // todo: use observable that does not need an app instance or logger
            _observable = new ObservableBase<StateChange>(NullLogger.Instance, null!);
        }

        private void NetDaemonHostOnHassEvents(object? sender, HassEvent hassEvent)
        {
            if (hassEvent.Data is not HassStateChangedEventData stateChangedEventData) return;
            
            _entityStateCache.Store(stateChangedEventData.NewState!);

            foreach (var observer in _observable.Observers)
            {
                observer.OnNext(stateChangedEventData.Map(this));
            }
        }

        public EntityState? GetState(string entityId)
        {
            var hassState = _entityStateCache.GetState(entityId);
            
            return HassObjectMapperMapper.Map(hassState);
        }

        public void CallService(string domain, string service, object? data, bool waitForResponse = false)
        {
            _netDaemonHost.CallService(domain, service, data, waitForResponse);
        }

        //public IRxEvent EventChanges { get; }
        public IObservable<StateChange> StateAllChanges => _observable;
        public IObservable<StateChange> StateChanges => _observable.Where(e => e.New?.State != e.Old?.State);
    }
}
