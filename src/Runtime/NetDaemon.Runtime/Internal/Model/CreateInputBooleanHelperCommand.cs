using System.Text.Json.Serialization;
using NetDaemon.Client.HomeAssistant.Model;

namespace NetDaemon.Runtime.Internal.Model;

internal record CreateInputBooleanHelperCommand : CommandMessage
{
    public CreateInputBooleanHelperCommand()
    {
        Type = "input_boolean/create";
    }

    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
}

internal record DeleteInputBooleanHelperCommand : CommandMessage
{
    public DeleteInputBooleanHelperCommand()
    {
        Type = "input_boolean/delete";
    }

    [JsonPropertyName("input_boolean_id")] public string InputBooleanId { get; init; } = string.Empty;
}

internal record ListInputBooleanHelperCommand : CommandMessage
{
    public ListInputBooleanHelperCommand()
    {
        Type = "input_boolean/list";
    }
}
