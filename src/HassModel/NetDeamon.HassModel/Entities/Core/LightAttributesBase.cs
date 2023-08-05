namespace NetDaemon.HassModel.Entities.Core;
#pragma warning disable CS1591 

/// <summary>
///     Attributes base class for light entities.
/// </summary>
/// <remarks>
///     We use doubles even if the standard is integer to make the serializing more robust
///     since we cannot trust HA integrations use integers when supposed to
/// </remarks>
[Obsolete("Usage of attribute base classes are deprecated, default meta data is not used to replace it")]
public record LightAttributesBase
{
    /// <summary>
    /// Integer between 0 and 255 for how bright the light should be, where 0 means the light is off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.
    /// </summary>
    [JsonPropertyName("brightness")]
    public double? Brightness { get; init; }
    
    [JsonPropertyName("color_mode")]
    public string? ColorMode { get; init; }
    
    /// <summary>
    /// Integer in mireds representing the color temperature of the light.
    /// </summary>
    [JsonPropertyName("color_temp")]
    public double? ColorTemp { get; init; }

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
    
    /// <summary>
    /// Entity ids of the entities in the light group. Null if not a group.
    /// </summary>
    [JsonPropertyName("entity_id")]
    public IReadOnlyList<string>? EntityId { get; init; }

    /// <summary>
    /// Name of the light as displayed in the UI.
    /// </summary>
    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }
    
    /// <summary>
    /// List containing two floats representing the hue and saturation of the color of the light. Hue is scaled 0-360, and saturation is scaled 0-100.
    /// </summary>
    [JsonPropertyName("hs_color")]
    public IReadOnlyList<double>? HsColor { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }
    
    /// <summary>
    /// The warmest color_temp that this light supports.
    /// </summary>
    [JsonPropertyName("max_mireds")]
    public double? MaxMireds { get; init; }

    /// <summary>
    /// The coldest color_temp that this light supports.
    /// </summary>
    [JsonPropertyName("min_mireds")]
    public double? MinMireds { get; init; }

    /// <summary>
    /// List containing three integers between 0 and 255 representing the RGB color (red, green, blue) of the light. Three comma-separated integers that represent the color in RGB.
    /// </summary>
    [JsonPropertyName("rgb_color")]
    public IReadOnlyList<double>? RgbColor { get; init; }
    
    /// <summary>
    /// List containing four integers between 0 and 255 representing the RGBW color (red, green, blue, white) of the light. This attribute will be null for lights which do not support RGBW colors.
    /// </summary>
    [JsonPropertyName("rgbw_color")]
    public IReadOnlyList<double>? RgbwColor { get; init; }
    
    /// <summary>
    /// List containing five integers between 0 and 255 representing the RGBWW color (red, green, blue, cold white, warm white) of the light. This attribute will be null for lights which do not support RGBWW colors.
    /// </summary>
    [JsonPropertyName("rgbww_color")]
    public IReadOnlyList<double>? RgbwwColor { get; init; }
    
    [JsonPropertyName("supported_color_modes")]
    public IReadOnlyList<string>? SupportedColorModes { get; init; }
    
    [JsonPropertyName("supported_features")]
    public long? SupportedFeatures { get; init; }

    /// <summary>
    /// List containing two floats representing the xy color of the light. Two comma-separated floats that represent the color in XY.
    /// </summary>
    [JsonPropertyName("xy_color")]
    public IReadOnlyList<double>? XyColor { get; init; }
}
