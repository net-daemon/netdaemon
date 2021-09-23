using System.Text.Json.Serialization;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;

namespace NetDaemon.DevelopmentApps.Domains.Climate
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