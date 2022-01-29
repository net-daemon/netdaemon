namespace NetDaemon.Client.Internal.HomeAssistant.Messages;

internal record HassAuthMessage : HassMessageBase
{
    public HassAuthMessage()
    {
        Type = "auth";
    }

    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = string.Empty;
}
