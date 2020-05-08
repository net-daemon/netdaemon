using System.Text.Json.Serialization;

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service
{
    public class HostConfig
    {
        public HostConfig()
        {
        }

        [JsonPropertyName("token")]
        public string Token { get; set; } = "enter hass token here";

        [JsonPropertyName("host")]
        public string Host { get; set; } = "localhost";

        [JsonPropertyName("port")]
        public short Port { get; set; } = 8123;

        [JsonPropertyName("ssl")]
        public bool Ssl { get; set; } = false;

        [JsonPropertyName("source_folder")]
        public string? SourceFolder { get; set; } = null;

        [JsonPropertyName("generate_entities")]
        public bool? GenerateEntitiesOnStartup { get; set; } = false;
    }
}