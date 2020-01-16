using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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
            var entityState =  new EntityState()
            {
                EntityId = hassEvent.EntityId,
                State = hassEvent.State,
               
                LastUpdated = hassEvent.LastUpdated,
                LastChanged = hassEvent.LastChanged
            };
            foreach (var hassAttribute in hassEvent.Attributes)
            {
                ((IDictionary<String, Object>)entityState.Attribute)[hassAttribute.Key] =
                    hassAttribute.Value;
            }

            return entityState;
        }

        public static string ToCSharpString(string str)
        {
            StringBuilder builder = new StringBuilder(str.Length*2);

            bool nextAlphaCharShouldBeUpper = true; // First on should be upper char

            for (short i=0; i < str.Length; i++)
            {
                char c = str[i];
                if (char.IsLetter(c) && nextAlphaCharShouldBeUpper)
                {
                    builder.Append(char.ToUpper(str[i]));
                    nextAlphaCharShouldBeUpper = false;
                    continue;
                }
                    
                if (c == '_')
                {
                    nextAlphaCharShouldBeUpper = true;
                    continue;
                }

                builder.Append(c);
            }

            return builder.ToString();
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
