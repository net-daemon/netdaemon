namespace NetDaemon.Client.HomeAssistant.Model;

public record CommandMessage : HassMessageBase
{
    private static readonly JsonSerializerOptions NonIndentingJsonSerializerOptions = new() { WriteIndented = false };

    [JsonPropertyName("id")] public int Id { get; set; }

    public string GetJsonString() => JsonSerializer.Serialize(this, GetType(), NonIndentingJsonSerializerOptions);
}
