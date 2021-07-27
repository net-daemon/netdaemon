using JoySoftware.HomeAssistant.Model;
using NetDaemon.Common.ModelV3;

namespace Model3.ModelV3
{
    static class StateChangeMapper
    {
        static public StateChange Map(this HassStateChangedEventData source, IHaContext haContext)
        {
            return new StateChange(
                new Entity(haContext, source.EntityId),
                new EntityState(source.OldState!),
                new EntityState(source.NewState!));
        }
    }
}