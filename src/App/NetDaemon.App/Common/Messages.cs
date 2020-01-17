using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public class Event
    {
    }

    public class ServiceEvent : Event
    {
        public string Domain { get; set; } = "";

        public string Service { get; set; } = "";

        public JsonElement? ServiceData { get; set; } = null;
    }

    public class StateChangedEvent : Event
    {
        public string EntityId { get; set; } = "";

        public EntityState? OldState { get; set; } = null;

        public EntityState? NewState { get; set; } = null;
    }


    public class EntityState  : IEntityProperties
    {

        public string EntityId { get; set; } = "";

        public string State { get; set; } = "";

        public dynamic Attribute { get; set; } = new FluentExpandoObject(true, true);

        public DateTime LastChanged { get; set; } = DateTime.MinValue;

        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    }

    public class Config
    {
        public float? Latitude { get; set; } = null;

        public float? Longitude { get; set; } = null;

        public int? Elevation { get; set; } = null;

        public HassUnitSystem? UnitSystem { get; set; } = null;

        public string? LocationName { get; set; } = null;

        public string? TimeZone { get; set; } = null;

        public List<string>? Components { get; set; } = null;

        public string? ConfigDir { get; set; } = null;


        public List<string>? WhitelistExternalDirs { get; set; } = null;

        public string? Version { get; set; } = null;
    }

    public class HassUnitSystem
    {
        public string? Length { get; set; } = null;

        public string? Mass { get; set; } = null;

        public string? Temperature { get; set; } = null;

        public string? Volume { get; set; } = null;
    }
}