using System;
using System.Collections.Generic;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using System.Text;

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

    public class RxEntity : ITurnOn, ITurnOff, IToggle, ISetState
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

    }
}
