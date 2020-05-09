using System;
using System.Collections.Generic;
using System.Text;

namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{

    public interface ITurnOn
    {
        void TurnOn((string, object)[]? attributes);
    }
    public interface ITurnOff
    {
        void TurnOff((string, object)[]? attributes);
    }

    public interface IToggle
    {
        void Toggle((string, object)[]? attributes);
    }

    public class RxEntity : ITurnOn, ITurnOff, IToggle
    {
        private readonly INetDaemon _daemon;
        private readonly IEnumerable<string> _entityIds;

        public RxEntity(INetDaemon daemon, IEnumerable<string> entityIds)
        {
            _daemon = daemon;
            _entityIds = entityIds; 
        }

        public void TurnOff(params (string, object)[]? attributes) => CallServiceOnEntity("turn_off", attributes);
        public void TurnOn(params (string, object)[]? attributes) => CallServiceOnEntity("turn_on", attributes);
        public void Toggle(params (string, object)[]? attributes) => CallServiceOnEntity("toggle", attributes);

        internal static string GetDomainFromEntity(string entity)
        {
            var entityParts = entity.Split('.');
            if (entityParts.Length != 2)
                throw new ApplicationException($"entity_id is mal formatted {entity}");

            return entityParts[0];
        }

        private void CallServiceOnEntity(string service, (string, object)[]? attributes = null)
        {
            
            foreach (var entityId in _entityIds)
            {
                var serviceData = new FluentExpandoObject();
                
                // Add attributes to dynamic object
                if (attributes is object)
                    foreach (var (attributeName, attributValue) in attributes)
                    {
                        serviceData[attributeName] = attributValue;
                    }

                var domain = GetDomainFromEntity(entityId);
               
                serviceData["entity_id"] = entityId;

                _daemon.CallService(domain, service, serviceData);
            }

        }

    }
}
