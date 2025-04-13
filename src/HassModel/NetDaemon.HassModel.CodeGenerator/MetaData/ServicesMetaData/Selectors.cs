using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record Selector()
{
    public bool Multiple { get; init; }

    public string? Type { get; init; }
}


internal record AreaSelector : Selector;

internal record DeviceSelector : Selector;

internal record EntitySelector : Selector
{
    [JsonConverter(typeof(StringAsArrayConverter))]
    public string[] Domain { get; init; } = [];
}

internal record NumberSelector : Selector
{
    public double? Step { get; init; }
}

internal record TargetSelector : Selector
{
    [JsonConverter(typeof(SingleObjectAsArrayConverter<EntitySelector>))]
    public EntitySelector[] Entity { get; init; } = [];
}

