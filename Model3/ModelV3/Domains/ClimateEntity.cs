using System;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using Model3;

namespace NetDaemon.Common.ModelV3.Domains
{
    public record ClimateEntity(IHaContext HaContext, string EntityId) 
        : Entity<ClimateEntity, ClimateState>(HaContext, EntityId)
    { }

    public record ClimateState(EntityState Source) : EntityState<ClimateAttributes>(Source);
    
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