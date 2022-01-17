using System.Text.Json.Serialization;
using NetDaemon.Client.Common.HomeAssistant.Model;

namespace NetDaemon.Runtime.Internal.Model;

internal record CreateInputBooleanHelperCommand : CommandMessage
{
    public CreateInputBooleanHelperCommand()
    {
        Type = "input_boolean/create";
    }

    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
}