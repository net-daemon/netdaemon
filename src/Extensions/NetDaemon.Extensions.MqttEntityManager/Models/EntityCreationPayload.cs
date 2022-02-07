using System.Text.Json.Serialization;

namespace NetDaemon.Extensions.MqttEntityManager.Models;

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

    [JsonPropertyName("command_topic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CommandTopic { get; set; }
    
    [JsonPropertyName("state_topic")]
    public string? StateTopic { get; set; }

    [JsonPropertyName("json_attributes_topic")]
    public string? JsonAttributesTopic { get; set; }
}