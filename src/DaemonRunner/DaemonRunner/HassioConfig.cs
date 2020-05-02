using System.Text.Json.Serialization;

public class HassioConfig
{

    [JsonPropertyName("log_level")]
    public string? LogLevel { get; set; }

    [JsonPropertyName("generate_entities")]
    public bool? GenerateEntitiesOnStart { get; set; }
}