using System;
using System.Collections;
using System.Globalization;
using System.Text.Json;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.Common.ModelV3
{
    public record EntityState
    {
        public EntityState()
        { }
        
        public EntityState(HassState hassState)
        {
            EntityId = hassState.EntityId;
            AttributesJson = hassState.AttributesJson ?? new JsonElement();
            LastChanged = hassState.LastChanged;
            LastUpdated = hassState.LastUpdated;
            Context = new Context
            {
                Id = hassState.Context.Id,
                UserId = hassState.Context.UserId,
                ParentId = hassState.Context.UserId,
            };
        }

        public string EntityId { get; } = "";

        /// <summary>
        ///     The state
        /// </summary>
        public string State { get; } = "";

        public JsonElement AttributesJson { get; }
        /// <summary>
        ///     Unique id of the entity
        /// </summary>

        /// <summary>
        ///     Attributes of the entity
        /// </summary>
        public virtual object? Attributes { get; init; } = new FluentExpandoObject(true, true);

        
        /// <summary>
        ///     Last changed, when state changed from and to different values
        /// </summary>
        public DateTime LastChanged { get; }

        /// <summary>
        ///     Last updated, when entity state or attributes changed
        /// </summary>
        public DateTime LastUpdated { get; }

        /// <summary>
        /// Context
        /// </summary>
        public Context? Context { get; }
    }
}