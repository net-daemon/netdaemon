namespace NetDaemon.HassModel.Internal;

internal static class HassObjectMapper
{
    public static EntityState? Map(this HassState? hassState)
    {
        return hassState == null
            ? null
            : new EntityState
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
        return target is null
            ? null
            : new HassTarget
            {
                AreaIds = target.AreaIds,
                DeviceIds = target.DeviceIds,
                EntityIds = target.EntityIds,
                FloorIds = target.FloorIds,
                LabelIds = target.LabelIds
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
            Floor = hassArea.FloorId is null ? null : registry.GetFloor(hassArea.FloorId),
        };
    }

    public static Device Map(this HassDevice hassDevice, IHaRegistryNavigator registry)
    {
        return new Device(registry)
        {
            Name = hassDevice.Name,
            NameByUser = hassDevice.NameByUser,
            Id = hassDevice.Id ?? "Unavailable",
            Area = hassDevice.AreaId is null ? null : registry.GetArea(hassDevice.AreaId),
            Labels = hassDevice.Labels.Select(registry.GetLabel).OfType<Label>().ToList(),
            Manufacturer = hassDevice.Manufacturer,
            Model = hassDevice.Model,
            ConfigurationUrl = hassDevice.ConfigurationUrl,
            SerialNumber = hassDevice.SerialNumber,
            HardwareVersion = hassDevice.HardwareVersion,
            SoftwareVersion = hassDevice.SoftwareVersion,
            Identifiers = hassDevice.Identifiers.Where(x => x.Count == 2).Select(id => (id[0], id[1])).ToList()
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
            Id = hassEntity.EntityId,
            Name = hassEntity.Name,
            Area = areaId is null ? null : registry.GetArea(areaId),
            Device = device,
            Labels = hassEntity.Labels.Select(registry.GetLabel).OfType<Label>().ToList(),
            Platform = hassEntity.Platform,
            Options = hassEntity.Options?.Map()
        };
    }

    private static EntityOptions Map(this HassEntityOptions entityOptions)
    {
        return new EntityOptions { ConversationOptions = entityOptions.Conversation?.Map() };
    }

    private static ConversationOptions Map(this HassEntityConversationOptions options)
    {
        return new ConversationOptions { ShouldExpose = options.ShouldExpose };
    }
}
