using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Model3.ModelV3;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon;
using NetDaemon.Mapping;

namespace NetDaemon.Common.ModelV3
{
    // TODO: try to derive from a simpler base class or interface if possible so we require as little as possible
    public class NdApplication : NetDaemonAppBase, IHandleHassEvent, IHaContext
    {
        private readonly ObservableBase<StateChange> _observable;
        
        // TODO: the state cache should be share between applications
        private readonly EntityStateCache _entityStateCache = new ();
        public NdApplication()
        {
            _observable = new ObservableBase<StateChange>(Logger?? NullLogger.Instance, this);
        }


        public EntityState? GetState(string entityId)
        {
            var hassState = _entityStateCache.GetState(entityId);
            
            return hassState == null ? null : new EntityState(hassState);
        }

        public void CallService(string domain, string service, object? data, bool waitForResponse = false)
        {
            throw new NotImplementedException();
        }

        //public IRxEvent EventChanges { get; }
        public IObservable<StateChange> StateAllChanges => _observable;
        public IObservable<StateChange> StateChanges => _observable.Where(e => e.New?.State != e.Old?.State);

        public void Initialize(IHassClient client)
        {
            // todo: better way to initialize cache 
            _entityStateCache.RefreshAsync(client).Wait();        
        }

        public Task HandleNewEvent(HassEvent hassEvent)
        {
            if (hassEvent.Data is not HassStateChangedEventData stateChangedEventData) return Task.CompletedTask;
            
            _entityStateCache.Store(stateChangedEventData.NewState!);
            
            foreach (var observer in _observable.Observers)
            {
                observer.OnNext(stateChangedEventData.Map(this));
            }
            
            return Task.CompletedTask;
        }
    }
}
