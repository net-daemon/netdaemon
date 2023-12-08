using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record Selector()
{
    public bool Multiple { get; init; }

    public string? Type { get; init; }
}


internal record AreaSelector : Selector
{
    public DeviceSelector? Device { get; init; }

    public EntitySelector? Entity { get; init; }
}

internal record DeviceSelector : Selector
{
    public string? Integration { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public EntitySelector? Entity { get; init; }
}

internal record EntitySelector : Selector
{
    public string? Integration { get; init; }

    [JsonConverter(typeof(StringAsArrayConverter))]
    public string[] Domain { get; init; } = Array.Empty<string>();
}

internal record NumberSelector : Selector
{
    public double? Min { get; init; }

    public double? Max { get; init; }

    public double? Step { get; init; }

    public string? UnitOfMeasurement { get; init; }
}

internal record TargetSelector : Selector
{
    [JsonConverter(typeof(SingleObjectAsArrayConverter<EntitySelector>))]
    public EntitySelector[] Entity { get; init; } = Array.Empty<EntitySelector>();
}

