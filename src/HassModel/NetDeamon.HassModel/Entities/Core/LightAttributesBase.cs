namespace NetDaemon.HassModel.Entities.Core;

public record LightAttributesBase
{
    [JsonPropertyName("entity_id")] public object? EntityId { get; init; }

    /// <summary>
    /// Integer between 0 and 255 for how bright the light should be, where 0 means the light is off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.
    /// </summary>
    [JsonPropertyName("brightness")] public int? Brightness { get; init; }

    [JsonPropertyName("color_mode")] public string? ColorMode { get; init; }

    [JsonPropertyName("effect_list")] public IReadOnlyList<string>? EffectList { get; init; }

    [JsonPropertyName("friendly_name")] public string? FriendlyName { get; init; }

    [JsonPropertyName("icon")] public string? Icon { get; init; }
    
    [JsonPropertyName("max_mireds")] public double? MaxMireds { get; init; }

    [JsonPropertyName("min_mireds")] public double? MinMireds { get; init; }

    [JsonPropertyName("off_brightness")] public object? OffBrightness { get; init; }

    [JsonPropertyName("supported_color_modes")]
    public IReadOnlyList<string>? SupportedColorModes { get; init; }

    [JsonPropertyName("supported_features")]
    public double? SupportedFeatures { get; init; }
    
    [JsonPropertyName("color_temp")]
    public double? ColorTemp { get; init; }

    [JsonPropertyName("hs_color")]
    public double[]? HsColor { get; init; }

    [JsonPropertyName("rgb_color")]
    public double[]? RgbColor { get; init; }

    [JsonPropertyName("xy_color")]
    public double[]? XyColor { get; init; }
}