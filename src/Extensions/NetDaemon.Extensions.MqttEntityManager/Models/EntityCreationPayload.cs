using System.Text.Json.Serialization;

namespace NetDaemon.Extensions.MqttEntityManager.Models;

/// <summary>
/// The base payload used to create an entity via MTQQ
/// </summary>
internal class EntityCreationPayload
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("device_class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeviceClass { get; set; }

    [JsonPropertyName("unique_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UniqueId { get; set; }
    
    [JsonPropertyName("object_id")]
    public string? ObjectId { get; set; }

    [JsonPropertyName("command_topic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CommandTopic { get; set; }
    
    [JsonPropertyName("state_topic")]
    public string? StateTopic { get; set; }

    [JsonPropertyName("json_attributes_topic")]
    public string? JsonAttributesTopic { get; set; }

    [JsonPropertyName("availability_topic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AvailabilityTopic { get; set; }
    
    [JsonPropertyName("payload_available")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PayloadAvailable { get; set; }

    [JsonPropertyName("payload_not_available")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PayloadNotAvailable { get; set; }

    [JsonPropertyName("payload_on")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PayloadOn { get; set; }

    [JsonPropertyName("payload_off")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PayloadOff { get; set; }

}