using System.Text.Json.Serialization;
using NetDaemon.Client.HomeAssistant.Model;

namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

public record CreateInputBooleanHelperCommand : CommandMessage
{
    public CreateInputBooleanHelperCommand()
    {
        Type = "input_boolean/create";
    }

    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
}

public record DeleteInputBooleanHelperCommand : CommandMessage
{
    public DeleteInputBooleanHelperCommand()
    {
        Type = "input_boolean/delete";
    }

    [JsonPropertyName("input_boolean_id")] public string InputBooleanId { get; init; } = string.Empty;
}

public record ListInputBooleanHelperCommand : CommandMessage
{
    public ListInputBooleanHelperCommand()
    {
        Type = "input_boolean/list";
    }
}
