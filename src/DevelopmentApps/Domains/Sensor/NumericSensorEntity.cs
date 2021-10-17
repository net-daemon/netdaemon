using System.Text.Json.Serialization;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.DevelopmentApps.Domains.Sensor
{
    public record NumericSensorEntity : NumericEntity<NumericSensorAttributes>
    {
        public NumericSensorEntity(IHaContext hasscontext, string entityId) : base(hasscontext, entityId) { }
    }
    
    public record NumericSensorAttributes
    {
        [JsonPropertyName("unit_of_measurement")]
        public string? UnitOfMeasurement { get; init; }
        
        [JsonPropertyName("friendly_name")]
        public string? FriendlyName { get; init; }
    }
}