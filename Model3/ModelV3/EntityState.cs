using System;
using System.Collections.Generic;
using System.Text.Json;
using JoySoftware.HomeAssistant.Model;
using Model3;

namespace NetDaemon.Common.ModelV3
{
    public record EntityState
    {
        public EntityState()
        { }
        
        public EntityState(HassState hassState)
        {
            // TODO: move to a mapper class
            EntityId = hassState.EntityId;
            AttributesJson = hassState.AttributesJson ?? new JsonElement();
            LastChanged = hassState.LastChanged;
            LastUpdated = hassState.LastUpdated;
            Context = hassState.Context == null ? null : 
                            new Context
                            {
                                Id = hassState.Context.Id,
                                UserId = hassState.Context.UserId,
                                ParentId = hassState.Context.UserId,
                            };
        }

        public string EntityId { get; } = "";

        public string State { get; } = "";

        public JsonElement AttributesJson { get; }
        public virtual object Attributes => AttributesJson.ToObject<Dictionary<string, object>>();

        public DateTime LastChanged { get; }

        public DateTime LastUpdated { get; }

        public Context? Context { get; }
    }
    
    public record EntityState<TAttributes> : EntityState where TAttributes : class
    {
        private Lazy<TAttributes> AttributesLazy;

        public EntityState(EntityState source) : base(source)
        {
            AttributesLazy = new (() => AttributesJson.ToObject<TAttributes>());            
        }

        public override TAttributes Attributes => AttributesLazy.Value;
    }

}