using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Internal.Client;

internal static class HassObjectMapper
{
    public static StateChange Map(this HassStateChangedEventData source, IHaContext haContext)
    {
        return new StateChange(
            new Entity(haContext, source.EntityId),
            Map(source.OldState),
            Map(source.NewState));
    }

    public static EntityState? Map(this HassState? hassState)
    {
        if (hassState == null) return null;

        return new EntityState
        {
            EntityId = hassState.EntityId,
            State = hassState.State,
            AttributesJson = hassState.AttributesJson,
            LastChanged = hassState.LastChanged,
            LastUpdated = hassState.LastUpdated,
            Context = hassState.Context == null
                ? null
                : new Context
                {
                    Id = hassState.Context.Id,
                    UserId = hassState.Context.UserId,
                    ParentId = hassState.Context.UserId
                }
        };
    }

    public static HassTarget? Map(this ServiceTarget? target)
    {
        if (target is null) return null;

        return new HassTarget
        {
            AreaIds = target.AreaIds,
            DeviceIds = target.DeviceIds,
            EntityIds = target.EntityIds
        };
    }

    public static Event Map(this HassEvent hassEvent)
    {
        return new Event
        {
            Origin = hassEvent.Origin,
            EventType = hassEvent.EventType,
            TimeFired = hassEvent.TimeFired,
            DataElement = hassEvent.DataElement
        };
    }

    public static Area Map(this HassArea hassArea)
    {
        return new Area
        {
            Name = hassArea.Name
        };
    }
}