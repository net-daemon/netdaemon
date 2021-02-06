using System.Text.Json.Serialization;

namespace NetDaemon.Infrastructure.Config
{
    public class HassioConfig
    {
        [JsonPropertyName("log_level")]
        public string? LogLevel { get; set; }

        [JsonPropertyName("app_source")]
        public string? AppSource { get; set; }

        [JsonPropertyName("log_messages")]
        public bool? LogMessages { get; set; }

        [JsonPropertyName("generate_entities")]
        public bool? GenerateEntitiesOnStart { get; set; }
    }
}