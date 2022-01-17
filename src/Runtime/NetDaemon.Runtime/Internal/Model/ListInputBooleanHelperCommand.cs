using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.Runtime.Internal.Model;

internal record ListInputBooleanHelperCommand : CommandMessage
{
    public ListInputBooleanHelperCommand()
    {
        Type = "input_boolean/list";
    }
}