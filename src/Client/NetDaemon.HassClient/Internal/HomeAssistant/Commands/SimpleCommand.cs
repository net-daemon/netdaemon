namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record SimpleCommand : CommandMessage
{
    public SimpleCommand(string type)
    {
        Type = type;
    }
}
