namespace NetDaemon.Client.HomeAssistant.Model;

public record CommandMessage : HassMessageBase
{
    [JsonPropertyName("id")] public int Id { get; set; }
}