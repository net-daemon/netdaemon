namespace NetDaemon.HassModel.Internal;

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

    public static Area Map(this HassArea hassArea, IHaRegistryNavigator registry)
    {
        return new Area(registry)
        {
            Name = hassArea.Name,
            Id = hassArea.Id,
            Labels = hassArea.Labels.Select(registry.GetLabel).OfType<Label>().ToList(),
            Floor = registry.GetFloor(hassArea.FloorId),
        };
    }

    public static Device Map(this HassDevice hassDevice, IHaRegistryNavigator registry)
    {
        return new Device(registry)
        {
            Name = hassDevice.Name,
            Id = hassDevice.Id ?? "Unavailable",
            Area = hassDevice.AreaId is null ? null : registry.GetArea(hassDevice.AreaId),
            Labels = hassDevice.Labels.Select(registry.GetLabel).OfType<Label>().ToList()
        };
    }

    public static Label Map(this HassLabel hassLabel, IHaRegistryNavigator registry)
    {
        return new Label(registry)
        {
            Name = hassLabel.Name,
            Id = hassLabel.Id ?? "Unavailable",
            Color = hassLabel.Color,
            Icon = hassLabel.Icon,
            Description = hassLabel.Description,
        };
    }

    public static Floor Map(this HassFloor hassFloor, IHaRegistryNavigator registry)
    {
        return new Floor(registry)
        {
            Name = hassFloor.Name,
            Id = hassFloor.Id ?? "Unavailable",
            Level = hassFloor.Level,
            Icon = hassFloor.Icon,
        };
    }
    public static EntityRegistration Map(this HassEntity hassEntity, IHaRegistryNavigator registry)
    {
        var device = hassEntity.DeviceId is null ? null : registry.GetDevice(hassEntity.DeviceId);
        var areaId = hassEntity.AreaId ?? device?.Area?.Id;

        return new EntityRegistration
        {
            Area = areaId is null ? null : registry.GetArea(areaId),
            Device = device,
            Labels = hassEntity.Labels.Select(registry.GetLabel).OfType<Label>().ToList(),
        };
    }

}
