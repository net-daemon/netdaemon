using System.Text.Json;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;

namespace NetDaemon.Model3.Internal
{
    internal static class HassObjectMapper
    {
        public static StateChange Map(this HassStateChangedEventData source, IHaContext haContext)
        {
            return new StateChange(
                new Entity(haContext, source.EntityId),
                Map(source.OldState),
                Map(source.NewState));
        }

        public static EntityState? Map(HassState? hassState)
        {
            if (hassState == null) return null;
            
            return new EntityState()
            {                
                EntityId = hassState.EntityId,
                State = hassState.State,
                AttributesJson = hassState.AttributesJson ?? new JsonElement(),
                LastChanged = hassState.LastChanged,
                LastUpdated = hassState.LastUpdated,
                Context = hassState.Context == null ? null : 
                    new Context
                    {
                        Id = hassState.Context.Id,
                        UserId = hassState.Context.UserId,
                        ParentId = hassState.Context.UserId,
                        }
                    };
        }
    }
}