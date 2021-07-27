using JoySoftware.HomeAssistant.Model;
using NetDaemon.Common.ModelV3;

namespace Model3.ModelV3
{
    static class StateChangeMapper
    {
        public static StateChange Map(this HassStateChangedEventData source, IHaContext haContext)
        {
            return new (
                new Entity(haContext, source.EntityId),
                new EntityState(source.OldState!),
                new EntityState(source.NewState!));
        }
    }
}