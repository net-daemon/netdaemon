namespace NetDaemon.Client.HomeAssistant.Model;

public record HassAuthResponse : HassMessageBase
{
    [JsonPropertyName("ha_version")] public string HaVersion { get; init; } = String.Empty;

}