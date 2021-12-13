namespace NetDaemon.Client.Common.HomeAssistant.Model
{
    /// <summary>
    ///     Represents a target for a service call in Home Assistant
    /// </summary>
    public record HassTarget
    {
        /// <summary>
        ///     Zero or more entity id to target with the service call
        /// </summary>
        [JsonPropertyName("entity_id")]
        public IReadOnlyCollection<string>? EntityIds { get; init; }

        /// <summary>
        ///     Zero or more device id to target with the service call
        /// </summary>
        [JsonPropertyName("device_id")]
        public IReadOnlyCollection<string>? DeviceIds { get; init; }

        /// <summary>
        ///     Zero or more area id to target with the service call
        /// </summary>
        [JsonPropertyName("area_id")]
        public IReadOnlyCollection<string>? AreaIds { get; init; }
    }
}