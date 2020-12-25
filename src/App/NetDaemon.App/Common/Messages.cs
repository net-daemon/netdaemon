using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using NetDaemon.Common.Fluent;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Home assistant config parameters
    /// </summary>
    public record Config
    {
        /// <summary>
        ///     Components being installed and used
        /// </summary>
        public List<string>? Components { get; init; } = null;

        /// <summary>
        ///     Local config directory
        /// </summary>
        public string? ConfigDir { get; init; } = null;

        /// <summary>
        ///     Elevation of Home Assistant instance
        /// </summary>
        public int? Elevation { get; init; } = null;

        /// <summary>
        ///     Latitude of Home Assistant instance
        /// </summary>
        public float? Latitude { get; init; } = null;

        /// <summary>
        ///     The name of the location
        /// </summary>
        public string? LocationName { get; init; } = null;

        /// <summary>
        ///     Longitude of Home Assistant instance
        /// </summary>
        public float? Longitude { get; init; } = null;

        /// <summary>
        ///     The timezone
        /// </summary>
        public string? TimeZone { get; init; } = null;

        /// <summary>
        ///     Unity system being configured
        /// </summary>
        public HassUnitSystem? UnitSystem { get; init; } = null;

        /// <summary>
        ///     Current Home Assistant version
        /// </summary>
        public string? Version { get; init; } = null;

        /// <summary>
        ///     Whitelisted external directories
        /// </summary>
        public List<string>? WhitelistExternalDirs { get; init; } = null;
    }

    /// <summary>
    ///     Detailed state information
    /// </summary>
    public record EntityState : IEntityProperties
    {
        /// <summary>
        ///     The name of the Area in home assistant
        /// </summary>
        public string? Area { get; init; }

        /// <summary>
        ///     Attributes of the entity
        /// </summary>
        public dynamic? Attribute { get; init; } = new FluentExpandoObject(true, true);

        /// <summary>
        ///     Unique id of the entity
        /// </summary>
        public string EntityId { get; init; } = "";

        /// <summary>
        ///     Last changed, when state changed from and to different values
        /// </summary>
        public DateTime LastChanged { get; init; } = DateTime.MinValue;

        /// <summary>
        ///     Last updated, when entity state or attributes changed
        /// </summary>
        public DateTime LastUpdated { get; init; } = DateTime.MinValue;

        /// <summary>
        ///     The state
        /// </summary>
        public dynamic? State { get; init; } = "";

        /// <summary>
        /// Context
        /// </summary>
        public Context? Context { get; init; }

        /// <summary>
        ///     returns a pretty print of EntityState
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string attributes = "";
            if (Attribute is IDictionary attr)
            {
                foreach (var key in attr.Keys)
                {
                    if (key is not null)
                    {
                        attributes += string.Format("       {0}:{1}", key, attr[key]);
                    }
                }
            }
            return string.Format(@"
                EntityId:       {0}
                State:          {1}
                Area:           {2}
                LastChanged:    {3}
                LastUpdated:    {4}
                Attributes:
                {5}
            ", EntityId, State, Area, LastChanged, LastUpdated, attributes);
        }
    }

    /// <summary>
    ///     Unit system parameters for Home Assistant
    /// </summary>
    public record HassUnitSystem
    {
        /// <summary>
        ///     Lenght unit
        /// </summary>
        public string? Length { get; init; } = null;

        /// <summary>
        ///     Mass unit
        /// </summary>
        public string? Mass { get; init; } = null;

        /// <summary>
        ///     Temperature unit
        /// </summary>
        public string? Temperature { get; init; } = null;

        /// <summary>
        ///     Volume unit
        /// </summary>
        public string? Volume { get; init; } = null;
    }

    /// <summary>
    ///     Event from CallService
    /// </summary>
    public record ServiceEvent
    {
        /// <summary>
        ///     Home Assistant domain
        /// </summary>
        public string Domain { get; init; } = "";

        /// <summary>
        ///     Service being called
        /// </summary>
        public string Service { get; init; } = "";

        /// <summary>
        ///     Data being sent by the service
        /// </summary>
        /// <value></value>
        public JsonElement? ServiceData { get; init; } = null;
    }
}