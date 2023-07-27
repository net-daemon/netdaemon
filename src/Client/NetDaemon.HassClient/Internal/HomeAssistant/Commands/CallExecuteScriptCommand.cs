namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record CallExecuteScriptCommand : CommandMessage
{
    public CallExecuteScriptCommand()
    {
        Type = "execute_script";
    }

    [JsonPropertyName("sequence")] public object[] Sequence { get; init; } = default!;
}