using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record ActionSelector
{
}

internal record AddonSelector
{
}

internal record AreaSelector
{
    public DeviceSelector? Device { get; init; }

    public EntitySelector? Entity { get; init; }
}

internal record BooleanSelector
{
}

internal record DeviceSelector
{
    public string? Integration { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public EntitySelector? Entity { get; init; }
}

internal record EntitySelector
{
    public string? Integration { get; init; }

    public string? Domain { get; init; }

    public string? DeviceClass { get; init; }
}

internal record NumberSelector
{
    [Required]
    public double Min { get; init; }

    [Required]
    public double Max { get; init; }

    public float? Step { get; init; }

    public string? UnitOfMeasurement { get; init; }

    [JsonConverter(typeof(NullableEnumStringConverter<NumberSelectorMode?>))]
    public NumberSelectorMode? Mode { get; init; }
}

internal enum NumberSelectorMode
{
    Box,
    Slider
}

internal record ObjectSelector
{
}

internal record SelectSelector
{
    [Required]
    public IReadOnlyCollection<string>? Options { get; init; }
}

internal record LabeledSelectSelector
{
    [JsonIgnore]
    public IReadOnlyCollection<string> Options { get { return _Options.Select(o => o.Value).ToList(); } init { } }

    [Required]
    [JsonPropertyName("options")]
    public IReadOnlyCollection<OptionSelector>? _Options { get; init; }
}

internal record OptionSelector
{
    [JsonPropertyName("label")]
    public string? Label { get; init; }
    [JsonPropertyName("value")]
    public string? Value { get; init; }
}

internal record TargetSelector
{
    public AreaSelector? Area { get; init; }

    public DeviceSelector? Device { get; init; }

    public EntitySelector? Entity { get; init; }
}

internal record TextSelector
{
    public bool? Multiline { get; init; }
}

internal record TimeSelector
{
}