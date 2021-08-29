using System;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using Model3;

namespace NetDaemon.Common.ModelV3.Domains
{
    public record ClimateEntity : Entity<ClimateEntity, EntityState<string, ClimateAttributes>, string, ClimateAttributes>
    {
        public ClimateEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
    }

    public record ClimateAttributes
    {
        // TODO: complete these props (this is really an example)
        [JsonPropertyName("temperature")]
        public double Temperature { get; init; }

        [JsonPropertyName("current_temperature")]
        public double CurrentTemperature { get; init; }
        
        [JsonPropertyName("hvac_action")]
        public string? HacAction { get; init; }
    }
}