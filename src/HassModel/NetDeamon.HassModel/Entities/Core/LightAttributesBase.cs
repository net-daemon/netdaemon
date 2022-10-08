namespace NetDaemon.HassModel.Entities.Core;
#pragma warning disable CS1591 

public record LightAttributesBase
{
    [JsonPropertyName("entity_id")]
    public object? EntityId { get; init; }

    /// <summary>
    /// Integer between 0 and 255 for how bright the light should be, where 0 means the light is off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.
    /// </summary>
    [JsonPropertyName("brightness")]
    public int? Brightness { get; init; }
    
    [JsonPropertyName("color_mode")]
    public string? ColorMode { get; init; }

    /// <summary>
    /// The current effect.
    /// </summary>
    [JsonPropertyName("effect")]
    public string? Effect { get; init; }
    
    /// <summary>
    /// The list of supported effects.
    /// </summary>
    [JsonPropertyName("effect_list")]
    public IReadOnlyList<string>? EffectList { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }
    
    /// <summary>
    /// The warmest color_temp that this light supports
    /// </summary>
    [JsonPropertyName("max_mireds")]
    public int? MaxMireds { get; init; }

    /// <summary>
    /// The coldest color_temp that this light supports.
    /// </summary>
    [JsonPropertyName("min_mireds")]
    public int? MinMireds { get; init; }

    [JsonPropertyName("off_brightness")]
    public object? OffBrightness { get; init; }
    
    [JsonPropertyName("supported_color_modes")]
    public IReadOnlyList<string>? SupportedColorModes { get; init; }
    
    [JsonPropertyName("supported_features")]
    public int? SupportedFeatures { get; init; }
    
    /// <summary>
    /// Integer in mireds representing the color temperature of the light.
    /// </summary>
    [JsonPropertyName("color_temp")]
    public int? ColorTemp { get; init; }

    /// <summary>
    /// List containing two floats representing the hue and saturation of the color of the light. Hue is scaled 0-360, and saturation is scaled 0-100.
    /// </summary>
    [JsonPropertyName("hs_color")]
    public IReadOnlyList<double>? HsColor { get; init; }

    /// <summary>
    /// List containing three integers between 0 and 255 representing the RGB color (red, green, blue) of the light. Three comma-separated integers that represent the color in RGB.
    /// </summary>
    [JsonPropertyName("rgb_color")]
    public IReadOnlyList<int>? RgbColor { get; init; }
    
    /// <summary>
    /// List containing four integers between 0 and 255 representing the RGBW color (red, green, blue, white) of the light. This attribute will be null for lights which do not support RGBW colors.
    /// </summary>
    [JsonPropertyName("rgbw_color")]
    public IReadOnlyList<int>? RgbwColor { get; init; }
    
    /// <summary>
    /// List containing five integers between 0 and 255 representing the RGBWW color (red, green, blue, cold white, warm white) of the light. This attribute will be null for lights which do not support RGBWW colors.
    /// </summary>
    [JsonPropertyName("rgbww_color")]
    public IReadOnlyList<int>? RgbwwColor { get; init; }

    /// <summary>
    /// List containing two floats representing the xy color of the light. Two comma-separated floats that represent the color in XY.
    /// </summary>
    [JsonPropertyName("xy_color")]
    public IReadOnlyList<double>? XyColor { get; init; }
}