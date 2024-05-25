namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record SupportedFeaturesCommand : CommandMessage
{
    public SupportedFeaturesCommand()
    {
        Type = "supported_features";
    }

    [JsonPropertyName("features")] public Features? Features { get; init; }
}

internal record Features
{
    [JsonPropertyName("coalesce_messages")] public short? CoalesceMessages { get; init; } = 1;
}
