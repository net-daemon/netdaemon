using System;
using System.Collections.Generic;
using System.Text.Json;
using JoySoftware.HomeAssistant.Client;
using NetDaemon.Infrastructure.Extensions;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Mapping
{
    public static class EntityStateMapper
    {
        /// <summary>
        ///     Converts HassState to EntityState
        /// </summary>
        /// <param name="hassState">HassState object to map</param>
        public static EntityState Map(this HassState hassState)
        {
            _ = hassState ??
               throw new NetDaemonArgumentNullException(nameof(hassState));
            var entityState = new EntityState
            {
                EntityId = hassState.EntityId,
                State = hassState.State,

                LastUpdated = hassState.LastUpdated,
                LastChanged = hassState.LastChanged,
                Context = ContextMapper.Map(hassState.Context)
            };

            MapAttributes(entityState, hassState);

            return entityState;
        }

        private static void MapAttributes(EntityState entityState, HassState hassState)
        {
            if (hassState.Attributes == null)
                return;

            // Cast so we can work with the expando object
            if (entityState.Attribute is not IDictionary<string, object> dict)
                throw new ArgumentNullException(nameof(dict), "Expando object should always be dictionary!");

            foreach (var (key, value) in hassState.Attributes)
            {
                if (value is JsonElement elem)
                {
                    var dynValue = elem.ConvertToDynamicValue();

                    if (dynValue != null)
                        dict[key] = dynValue;
                }
                else
                {
                    dict[key] = value;
                }
            }
        }
    }
}