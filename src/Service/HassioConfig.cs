using System.Text.Json.Serialization;

namespace Service
{
    public class HassioConfig
    {
        [JsonPropertyName("log_level")]
        public string? LogLevel { get; set; }

        [JsonPropertyName("app_source")]
        public string? AppSource { get; set; } = null;

        [JsonPropertyName("log_messages")]
        public bool? LogMessages { get; set; }

        [JsonPropertyName("generate_entities")]
        public bool? GenerateEntitiesOnStart { get; set; }
    }
}