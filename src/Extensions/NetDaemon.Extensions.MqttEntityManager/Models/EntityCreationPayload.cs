using System.Text.Json.Serialization;

namespace NetDaemon.Extensions.MqttEntityManager.Models;

/// <summary>
/// The base payload used to create an entity via MTQQ
/// </summary>
internal class EntityCreationPayload
{
    /// <summary>
    /// Gets or sets the display name of the entity.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the Home Assistant device class.
    /// </summary>
    [JsonPropertyName("device_class")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeviceClass { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    [JsonPropertyName("unique_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UniqueId { get; set; }

    /// <summary>
    /// Gets or sets the default entity id.
    /// </summary>
    [JsonPropertyName("default_entity_id")]
    public string? DefaultEntityId { get; set; }

    /// <summary>
    /// Gets or sets the MQTT command topic.
    /// </summary>
    [JsonPropertyName("command_topic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CommandTopic { get; set; }

    /// <summary>
    /// Gets or sets the MQTT state topic.
    /// </summary>
    [JsonPropertyName("state_topic")]
    public string? StateTopic { get; set; }

    /// <summary>
    /// Gets or sets the MQTT JSON attributes topic.
    /// </summary>
    [JsonPropertyName("json_attributes_topic")]
    public string? JsonAttributesTopic { get; set; }

    /// <summary>
    /// Gets or sets the MQTT availability topic.
    /// </summary>
    [JsonPropertyName("availability_topic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AvailabilityTopic { get; set; }

    /// <summary>
    /// Gets or sets the payload that marks the entity as available.
    /// </summary>
    [JsonPropertyName("payload_available")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PayloadAvailable { get; set; }

    /// <summary>
    /// Gets or sets the payload that marks the entity as not available.
    /// </summary>
    [JsonPropertyName("payload_not_available")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PayloadNotAvailable { get; set; }

    /// <summary>
    /// Gets or sets the payload that represents the on state.
    /// </summary>
    [JsonPropertyName("payload_on")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PayloadOn { get; set; }

    /// <summary>
    /// Gets or sets the payload that represents the off state.
    /// </summary>
    [JsonPropertyName("payload_off")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? PayloadOff { get; set; }

}
