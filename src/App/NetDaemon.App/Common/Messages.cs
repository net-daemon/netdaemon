using System;
using System.Collections.Generic;
using System.Text.Json;
using NetDaemon.Common.Fluent;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Home assistant config parameters
    /// </summary>
    public class Config
    {
        /// <summary>
        ///     Components being installed and used
        /// </summary>
        public List<string>? Components { get; set; } = null;

        /// <summary>
        ///     Local config directory
        /// </summary>
        public string? ConfigDir { get; set; } = null;

        /// <summary>
        ///     Elevation of Home Assistant instance
        /// </summary>
        public int? Elevation { get; set; } = null;

        /// <summary>
        ///     Latitude of Home Assistant instance
        /// </summary>
        public float? Latitude { get; set; } = null;

        /// <summary>
        ///     The name of the location
        /// </summary>
        public string? LocationName { get; set; } = null;

        /// <summary>
        ///     Longitude of Home Assistant instance
        /// </summary>
        public float? Longitude { get; set; } = null;

        /// <summary>
        ///     The timezone
        /// </summary>
        public string? TimeZone { get; set; } = null;

        /// <summary>
        ///     Unity system being configured
        /// </summary>
        public HassUnitSystem? UnitSystem { get; set; } = null;

        /// <summary>
        ///     Current Home Assistant version
        /// </summary>
        public string? Version { get; set; } = null;

        /// <summary>
        ///     Whitelisted external directories
        /// </summary>
        public List<string>? WhitelistExternalDirs { get; set; } = null;
    }

    /// <summary>
    ///     Detailed state information
    /// </summary>
    public class EntityState : IEntityProperties
    {
        /// <summary>
        ///     The name of the Area in home assistant
        /// </summary>
        public string? Area { get; set; }

        /// <summary>
        ///     Attributes of the entity
        /// </summary>
        public dynamic? Attribute { get; set; } = new FluentExpandoObject(true, true);

        /// <summary>
        ///     Unique id of the entity
        /// </summary>
        public string EntityId { get; set; } = "";

        /// <summary>
        ///     Last changed, when state changed from and to different values
        /// </summary>
        public DateTime LastChanged { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     Last updated, when entity state or attributes changed
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;

        /// <summary>
        ///     The state
        /// </summary>
        public dynamic? State { get; set; } = "";

        /// <summary>
        /// Context
        /// </summary>
        public Context? Context { get; set; }
    }

    /// <summary>
    ///     Unit system parameters for Home Assistant
    /// </summary>
    public class HassUnitSystem
    {
        /// <summary>
        ///     Lenght unit
        /// </summary>
        public string? Length { get; set; } = null;

        /// <summary>
        ///     Mass unit
        /// </summary>
        public string? Mass { get; set; } = null;

        /// <summary>
        ///     Temperature unit
        /// </summary>
        public string? Temperature { get; set; } = null;

        /// <summary>
        ///     Volume unit
        /// </summary>
        public string? Volume { get; set; } = null;
    }

    /// <summary>
    ///     Event from CallService
    /// </summary>
    public class ServiceEvent
    {
        /// <summary>
        ///     Home Assistant domain
        /// </summary>
        public string Domain { get; set; } = "";

        /// <summary>
        ///     Service being called
        /// </summary>
        public string Service { get; set; } = "";

        /// <summary>
        ///     Data being sent by the service
        /// </summary>
        /// <value></value>
        public JsonElement? ServiceData { get; set; } = null;
    }
}