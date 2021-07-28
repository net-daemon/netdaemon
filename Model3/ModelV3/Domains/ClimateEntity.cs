using System;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using Model3;

namespace NetDaemon.Common.ModelV3.Domains
{
    public class ClimateEntity : Entity<ClimateEntity, ClimateState>
    {
        public ClimateEntity(IHaContext daemon, string entityId) : base(daemon, entityId) { }

        protected override ClimateState MapState(EntityState state) => new(state);
    }

    public record ClimateState : EntityState<ClimateAttributes>
    {
        public ClimateState(EntityState source) : base(source) { }
    }
    
    public record ClimateAttributes
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; init; }

        [JsonPropertyName("current_temperature")]
        public double CurrentTemperature { get; init; }
        
        [JsonPropertyName("hvac_action")]
        public string? HacAction { get; init; }
    }
}