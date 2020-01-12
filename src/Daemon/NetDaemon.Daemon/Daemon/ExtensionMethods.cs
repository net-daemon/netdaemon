using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon
{
    internal static class ExtensionMethods
    {
        /// <summary>
        ///     Converts HassState to DaemonState
        /// </summary>
        /// <param name="hassEvent"></param>
        /// <returns></returns>
        public static EntityState ToDaemonEvent(this HassState hassEvent)
        {
            return new EntityState()
            {
                EntityId = hassEvent.EntityId,
                State = hassEvent.State,
                Attributes = hassEvent.Attributes,
                LastUpdated = hassEvent.LastUpdated,
                LastChanged = hassEvent.LastChanged
            };
        }

        public static dynamic ToDynamic(this (string name, object val)[] attributeNameValuePair)
        {
            // Convert the tuple name/value pair to tuple that can be serialized dynamically
            var attributes = new ExpandoObject();
            foreach (var (attribute, value) in attributeNameValuePair)
            {
                ((IDictionary<string, object>)attributes).Add(attribute, value);
            }

            dynamic result = attributes;
            return result;
        }
    }
}
