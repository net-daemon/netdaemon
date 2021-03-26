using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using NetDaemon.Common;

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
        public IList<string>? Components { get; }

        /// <summary>
        ///     Local config directory
        /// </summary>
        public string? ConfigDir { get; }

        /// <summary>
        ///     Elevation of Home Assistant instance
        /// </summary>
        public int? Elevation { get; }

        /// <summary>
        ///     Latitude of Home Assistant instance
        /// </summary>
        public float? Latitude { get; }

        /// <summary>
        ///     The name of the location
        /// </summary>
        public string? LocationName { get; }

        /// <summary>
        ///     Longitude of Home Assistant instance
        /// </summary>
        public float? Longitude { get; }

        /// <summary>
        ///     The timezone
        /// </summary>
        public string? TimeZone { get; }
        /// <summary>
        ///     Unity system being configured
        /// </summary>
        public HassUnitSystem? UnitSystem { get; init; }

        /// <summary>
        ///     Current Home Assistant version
        /// </summary>
        public string? Version { get; init; }

        /// <summary>
        ///     Whitelisted external directories
        /// </summary>
        public ReadOnlyCollection<string>? WhitelistExternalDirs { get; }
    }

    /// <summary>
    ///     Detailed state information
    /// </summary>
    public record EntityState : IEntityProperties
    {
        /// <summary>
        /// Create a new EntityState from an ExistingEntityState
        /// </summary>
        /// <param name="objectToCopy"></param>
        public EntityState(EntityState objectToCopy)
        {
            if (objectToCopy != null)
            {
                Area = objectToCopy.Area;
                Attribute = objectToCopy.Attribute;
                EntityId = objectToCopy.EntityId;
                LastChanged = objectToCopy.LastChanged;
                LastUpdated = objectToCopy.LastUpdated;
                State = objectToCopy.State;
                Context = objectToCopy.Context;
            }

        }
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
        public override string ToString()
        {
            string attributes = "";
            if (Attribute is IDictionary attr)
            {
                foreach (var key in attr.Keys)
                {
                    if (key is not null)
                    {
                        attributes += string.Format(CultureInfo.InvariantCulture, "       {0}:{1}", key, attr[key]);
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
        public string? Length { get; init; }

        /// <summary>
        ///     Mass unit
        /// </summary>
        public string? Mass { get; init; }

        /// <summary>
        ///     Temperature unit
        /// </summary>
        public string? Temperature { get; init; }

        /// <summary>
        ///     Volume unit
        /// </summary>
        public string? Volume { get; init; }
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
        public JsonElement? ServiceData { get; init; }
    }
}