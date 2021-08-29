using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Model3.ModelV3.EventTypes
{
    public record ZhaEventData
    {
        [JsonPropertyName("device_ieee")] public string? DeviceIeee { get; set; }
        [JsonPropertyName("unique_id")] public string? UniqueId { get; set; }
        [JsonPropertyName("endpoint_id")] public int? EndpointId { get; set; }
        [JsonPropertyName("endpoint_id")] public int? ClusterId { get; set; }
        [JsonPropertyName("command")] public string? Command { get; set; }
        [JsonPropertyName("args")] public IReadOnlyCollection<object>? Args { get; set; }
    }
}