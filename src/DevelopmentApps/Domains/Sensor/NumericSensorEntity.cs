using System.Text.Json.Serialization;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;

namespace NetDaemon.DevelopmentApps.Domains.Sensor
{
    public record NumericSensorEntity : Entity<NumericSensorEntity, EntityState<double, NumericSensorAttributes>, double, NumericSensorAttributes>
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