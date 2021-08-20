using System;
using System.Collections.Generic;
using System.Text.Json;
using JoySoftware.HomeAssistant.Model;
using Model3;

namespace NetDaemon.Common.ModelV3
{
    public record EntityState
    {
        public string EntityId { get; init; } = "";

        public string? State { get; init; }

        public JsonElement AttributesJson { get; init; }
        public virtual object Attributes => AttributesJson.ToObject<Dictionary<string, object>>();

        public DateTime LastChanged { get; init; }

        public DateTime LastUpdated { get; init; }

        public Context? Context { get; init; }
    }
    
    public record EntityState<TAttributes> : EntityState where TAttributes : class
    {
        private readonly Lazy<TAttributes> _attributesLazy;

        public EntityState(EntityState source) : base(source)
        {
            _attributesLazy = new (() => AttributesJson.ToObject<TAttributes>());            
        }

        public override TAttributes Attributes => _attributesLazy.Value;
    }

}