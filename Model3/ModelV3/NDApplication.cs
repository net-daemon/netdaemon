using System;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Model;
using Model3.ModelV3;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon;
using NetDaemon.Mapping;

namespace NetDaemon.Common.ModelV3
{
    public class NdApplication : NetDaemonAppBase, IHandleHassEvent, IHaContext
    {
        private readonly ObservableBase<StateChange> _observable;
        private EntityStateCache _entityStateCache = new EntityStateCache();
        public NdApplication()
        {
            _observable = new ObservableBase<StateChange>(Logger, this);
        }
        public IObservable<StateChange> StateChanges { get; }

        public EntityState GetState(string entityId) => new (_entityStateCache.GetState(entityId));
        
        public void CallService(string domain, string service, object? data, bool waitForResponse = false)
        {
            throw new NotImplementedException();
        }


        public IRxEvent EventChanges { get; }
        IObservable<StateChange> IHaContext.StateAllChanges => StateAllChanges;

        IObservable<StateChange> StateAllChanges => _observable;

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
