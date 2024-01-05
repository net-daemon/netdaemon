namespace NetDaemon.Client.Internal.HomeAssistant.Commands;

internal record CreateInputBooleanHelperCommand : CommandMessage
{
    public CreateInputBooleanHelperCommand()
    {
        Type = "input_boolean/create";
    }

    [JsonPropertyName("name")] public required string Name { get; init; }
}

internal record DeleteInputBooleanHelperCommand : CommandMessage
{
    public DeleteInputBooleanHelperCommand()
    {
        Type = "input_boolean/delete";
    }

    [JsonPropertyName("input_boolean_id")] public required string InputBooleanId { get; init; } = string.Empty;
}

internal record ListInputBooleanHelperCommand : CommandMessage
{
    public ListInputBooleanHelperCommand()
    {
        Type = "input_boolean/list";
    }
}
