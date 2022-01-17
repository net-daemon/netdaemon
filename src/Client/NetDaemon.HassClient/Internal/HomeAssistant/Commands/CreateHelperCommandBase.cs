namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record CreateHelperCommandBase : CommandMessage
{
    public CreateHelperCommandBase(string helperType)
    {
        Type = $"{helperType}/create";
    }
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("icon")] public string Icon { get; init; } = string.Empty;
}