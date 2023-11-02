using System.ComponentModel.DataAnnotations;
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
    [Required]
    public double Min { get; init; }

    [Required]
    public double Max { get; init; }


    // Step can also contain the string "any" which is not usefull for our purpose, se we deserialize as a string and then try to parse as a double
    [JsonPropertyName("step")]
    public string? StepValue { get; init; }

    [JsonIgnore]
    public double? Step => double.TryParse(StepValue, out var d) ? d: null;

    public string? UnitOfMeasurement { get; init; }
}

internal record TargetSelector : Selector
{
    [JsonConverter(typeof(SingleObjectAsArrayConverter<EntitySelector>))]
    public EntitySelector[] Entity { get; init; } = Array.Empty<EntitySelector>();
}

