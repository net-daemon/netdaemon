using System;
using System.Collections.Generic;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using System.Text;
using System.Reactive.Linq;
using JoySoftware.HomeAssistant.Client;
using System.Linq;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{

    public interface ITurnOn
    {
        void TurnOn(dynamic? attributes);
    }
    public interface ITurnOff
    {
        void TurnOff(dynamic? attributes);
    }

    public interface IToggle
    {
        void Toggle(dynamic? attributes);
    }

    public interface ISetState
    {
        void SetState(dynamic state, dynamic? attributes);
    }

    public interface IObserve
    {
        IObservable<(EntityState Old, EntityState New)> StateChanges { get; }
        IObservable<(EntityState Old, EntityState New)> StateAllChanges { get; }

    }

    public class RxEntity : ITurnOn, ITurnOff, IToggle, ISetState, IObserve
    {
        private readonly INetDaemon _daemon;
        private readonly IEnumerable<string> _entityIds;

        public RxEntity(INetDaemon daemon, IEnumerable<string> entityIds)
        {
            _daemon = daemon;
            _entityIds = entityIds; 
        }

        public void TurnOff(dynamic? attributes) => CallServiceOnEntity("turn_off", attributes);
        public void TurnOn(dynamic? attributes) => CallServiceOnEntity("turn_on", attributes);
        public void Toggle(dynamic? attributes) => CallServiceOnEntity("toggle", attributes);
        public void SetState(dynamic state, dynamic? attributes)
        {
            foreach (var entityId in _entityIds)
            {
                var domain = GetDomainFromEntity(entityId);
                _daemon.SetState(entityId, state, attributes);
            }
        }

        internal static string GetDomainFromEntity(string entity)
        {
            var entityParts = entity.Split('.');
            if (entityParts.Length != 2)
                throw new ApplicationException($"entity_id is mal formatted {entity}");

            return entityParts[0];
        }

        private void CallServiceOnEntity(string service, dynamic? attributes = null)
        {
            dynamic? data=null; 

            if (attributes is object )
            {
                if (attributes is IDictionary<string, object?> == false)
                    data = ((object)attributes).ToExpandoObject();
                else
                    data = attributes;
            }

            foreach (var entityId in _entityIds)
            {
                var serviceData = new FluentExpandoObject();
                // Maske sure we make a copy since we reuse all info but entity id
                serviceData.CopyFrom(data);

                var domain = GetDomainFromEntity(entityId);
               
                serviceData["entity_id"] = entityId;

                _daemon.CallService(domain, service, serviceData);
            }

        }

        public IDisposable Subscribe(IObserver<(EntityState Old, EntityState New)> observer) => throw new NotImplementedException();

        public IObservable<(EntityState Old, EntityState New)> StateChanges
        {
            get
            {
                if (_entityIds.Count() > 1)
                {
                    var resultList
                        = new List<IObservable<(EntityState Old, EntityState New)>>();

                    foreach (var entity in _entityIds)
                    {
                        resultList.Add(_daemon.Where(f => f.New.EntityId == entity && f.New.State != f.Old.State));
                    }
                    return Observable.Merge(resultList);
                }
                else if (_entityIds.Count() == 1)
                {
                    return _daemon.Where(f => f.New.EntityId == _entityIds.First() && f.New.State != f.Old.State);
                }

                throw new Exception("Merge not allowed. No entity id:s were selected. Check your lambda query");
            }
        }

        public IObservable<(EntityState Old, EntityState New)> StateAllChanges 
        {
            get
            {
                var resultList
                    = new List<IObservable<(EntityState Old, EntityState New)>>();

                if (_entityIds.Count() > 1)
                {
                    foreach (var entity in _entityIds)
                    {
                        resultList.Add(_daemon.Where(f => f.New.EntityId == entity));
                    }
                    return Observable.Merge(resultList);
                }
                else if (_entityIds.Count() == 1)
                {
                    return _daemon.Where(f => f.New.EntityId == _entityIds.First());
                }

                throw new Exception("StateAllChanges not allowed. No entity id:s were selected. Check your lambda query");
            }
        }
    }
}
