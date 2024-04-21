using System.Text.Json.Serialization;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace MyLibrary;

public partial record LightEntity : Entity<LightAttributes>, ILightEntityCore
{
    public LightEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public LightEntity(IEntityCore entity) : base(entity)
    {
    }
}

public partial record LightAttributes
{
    [JsonPropertyName("min_color_temp_kelvin")]
    public double? MinColorTempKelvin { get; init; }

    [JsonPropertyName("max_color_temp_kelvin")]
    public double? MaxColorTempKelvin { get; init; }

    [JsonPropertyName("color_temp_kelvin")]
    public double? ColorTempKelvin { get; init; }

    [JsonPropertyName("off_with_transition")]
    public bool? OffWithTransition { get; init; }

    [JsonPropertyName("off_brightness")]
    public double? OffBrightness { get; init; }

    [JsonPropertyName("restored")]
    public bool? Restored { get; init; }

    [JsonPropertyName("supported_color_modes")]
    public IReadOnlyList<string>? SupportedColorModes { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }

    [JsonPropertyName("supported_features")]
    public double? SupportedFeatures { get; init; }

    [JsonPropertyName("min_mireds")]
    public double? MinMireds { get; init; }

    [JsonPropertyName("max_mireds")]
    public double? MaxMireds { get; init; }

    [JsonPropertyName("effect_list")]
    public IReadOnlyList<string>? EffectList { get; init; }

    [JsonPropertyName("entity_id")]
    public IReadOnlyList<string>? EntityId { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonPropertyName("color_mode")]
    public string? ColorMode { get; init; }

    [JsonPropertyName("brightness")]
    public double? Brightness { get; init; }

    [JsonPropertyName("color_temp")]
    public double? ColorTemp { get; init; }

    [JsonPropertyName("hs_color")]
    public IReadOnlyList<double>? HsColor { get; init; }

    [JsonPropertyName("rgb_color")]
    public IReadOnlyList<double>? RgbColor { get; init; }

    [JsonPropertyName("xy_color")]
    public IReadOnlyList<double>? XyColor { get; init; }
}


public partial record LightToggleParameters
{
    ///<summary>Duration it takes to get to next state.</summary>
    [JsonPropertyName("transition")]
    public long? Transition { get; init; }

    ///<summary>Color for the light in RGB-format. eg: [255, 100, 100]</summary>
    [JsonPropertyName("rgb_color")]
    public object? RgbColor { get; init; }

    ///<summary>A human readable color name.</summary>
    [JsonPropertyName("color_name")]
    public object? ColorName { get; init; }

    ///<summary>Color for the light in hue/sat format. Hue is 0-360 and Sat is 0-100. eg: [300, 70]</summary>
    [JsonPropertyName("hs_color")]
    public object? HsColor { get; init; }

    ///<summary>Color for the light in XY-format. eg: [0.52, 0.43]</summary>
    [JsonPropertyName("xy_color")]
    public object? XyColor { get; init; }

    ///<summary>Color temperature for the light in mireds.</summary>
    [JsonPropertyName("color_temp")]
    public object? ColorTemp { get; init; }

    ///<summary>Color temperature for the light in Kelvin.</summary>
    [JsonPropertyName("kelvin")]
    public long? Kelvin { get; init; }

    ///<summary>Number indicating brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</summary>
    [JsonPropertyName("brightness")]
    public long? Brightness { get; init; }

    ///<summary>Number indicating percentage of full brightness, where 0 turns the light off, 1 is the minimum brightness and 100 is the maximum brightness supported by the light.</summary>
    [JsonPropertyName("brightness_pct")]
    public long? BrightnessPct { get; init; }

    ///<summary>Name of a light profile to use. eg: relax</summary>
    [JsonPropertyName("profile")]
    public string? Profile { get; init; }

    ///<summary>If the light should flash.</summary>
    [JsonPropertyName("flash")]
    public object? Flash { get; init; }

    ///<summary>Light effect.</summary>
    [JsonPropertyName("effect")]
    public string? Effect { get; init; }
}

public partial record LightTurnOffParameters
{
    ///<summary>Duration it takes to get to next state.</summary>
    [JsonPropertyName("transition")]
    public long? Transition { get; init; }

    ///<summary>If the light should flash.</summary>
    [JsonPropertyName("flash")]
    public object? Flash { get; init; }
}

public partial record LightTurnOnParameters
{
    ///<summary>Duration it takes to get to next state.</summary>
    [JsonPropertyName("transition")]
    public long? Transition { get; init; }

    ///<summary>The color for the light (based on RGB - red, green, blue).</summary>
    [JsonPropertyName("rgb_color")]
    public object? RgbColor { get; init; }

    ///<summary>A list containing four integers between 0 and 255 representing the RGBW (red, green, blue, white) color for the light. eg: [255, 100, 100, 50]</summary>
    [JsonPropertyName("rgbw_color")]
    public object? RgbwColor { get; init; }

    ///<summary>A list containing five integers between 0 and 255 representing the RGBWW (red, green, blue, cold white, warm white) color for the light. eg: [255, 100, 100, 50, 70]</summary>
    [JsonPropertyName("rgbww_color")]
    public object? RgbwwColor { get; init; }

    ///<summary>A human readable color name.</summary>
    [JsonPropertyName("color_name")]
    public object? ColorName { get; init; }

    ///<summary>Color for the light in hue/sat format. Hue is 0-360 and Sat is 0-100. eg: [300, 70]</summary>
    [JsonPropertyName("hs_color")]
    public object? HsColor { get; init; }

    ///<summary>Color for the light in XY-format. eg: [0.52, 0.43]</summary>
    [JsonPropertyName("xy_color")]
    public object? XyColor { get; init; }

    ///<summary>Color temperature for the light in mireds.</summary>
    [JsonPropertyName("color_temp")]
    public object? ColorTemp { get; init; }

    ///<summary>Color temperature for the light in Kelvin.</summary>
    [JsonPropertyName("kelvin")]
    public long? Kelvin { get; init; }

    ///<summary>Number indicating brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</summary>
    [JsonPropertyName("brightness")]
    public long? Brightness { get; init; }

    ///<summary>Number indicating percentage of full brightness, where 0 turns the light off, 1 is the minimum brightness and 100 is the maximum brightness supported by the light.</summary>
    [JsonPropertyName("brightness_pct")]
    public long? BrightnessPct { get; init; }

    ///<summary>Change brightness by an amount.</summary>
    [JsonPropertyName("brightness_step")]
    public long? BrightnessStep { get; init; }

    ///<summary>Change brightness by a percentage.</summary>
    [JsonPropertyName("brightness_step_pct")]
    public long? BrightnessStepPct { get; init; }

    ///<summary>Set the light to white mode and change its brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</summary>
    [JsonPropertyName("white")]
    public long? White { get; init; }

    ///<summary>Name of a light profile to use. eg: relax</summary>
    [JsonPropertyName("profile")]
    public string? Profile { get; init; }

    ///<summary>If the light should flash.</summary>
    [JsonPropertyName("flash")]
    public object? Flash { get; init; }

    ///<summary>Light effect.</summary>
    [JsonPropertyName("effect")]
    public string? Effect { get; init; }
}


public static class LightEntityExtensionMethods
{
    ///<summary>Toggles one or more lights, from on to off, or, off to on, based on their current state. </summary>
    public static void Toggle(this ILightEntityCore target, LightToggleParameters data)
    {
        target.CallService("toggle", data);
    }

    ///<summary>Toggles one or more lights, from on to off, or, off to on, based on their current state. </summary>
    public static void Toggle(this IEnumerable<ILightEntityCore> target, LightToggleParameters data)
    {
        target.CallService("toggle", data);
    }

    ///<summary>Toggles one or more lights, from on to off, or, off to on, based on their current state. </summary>
    ///<param name="target">The ILightEntity to call this service for</param>
    ///<param name="transition">Duration it takes to get to next state.</param>
    ///<param name="rgbColor">Color for the light in RGB-format. eg: [255, 100, 100]</param>
    ///<param name="colorName">A human readable color name.</param>
    ///<param name="hsColor">Color for the light in hue/sat format. Hue is 0-360 and Sat is 0-100. eg: [300, 70]</param>
    ///<param name="xyColor">Color for the light in XY-format. eg: [0.52, 0.43]</param>
    ///<param name="colorTemp">Color temperature for the light in mireds.</param>
    ///<param name="kelvin">Color temperature for the light in Kelvin.</param>
    ///<param name="brightness">Number indicating brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</param>
    ///<param name="brightnessPct">Number indicating percentage of full brightness, where 0 turns the light off, 1 is the minimum brightness and 100 is the maximum brightness supported by the light.</param>
    ///<param name="profile">Name of a light profile to use. eg: relax</param>
    ///<param name="flash">If the light should flash.</param>
    ///<param name="effect">Light effect.</param>
    public static void Toggle(this ILightEntityCore target, long? transition = null, object? rgbColor = null, object? colorName = null, object? hsColor = null, object? xyColor = null, object? colorTemp = null, long? kelvin = null, long? brightness = null, long? brightnessPct = null, string? profile = null, object? flash = null, string? effect = null)
    {
        target.CallService("toggle", new LightToggleParameters { Transition = transition, RgbColor = rgbColor, ColorName = colorName, HsColor = hsColor, XyColor = xyColor, ColorTemp = colorTemp, Kelvin = kelvin, Brightness = brightness, BrightnessPct = brightnessPct, Profile = profile, Flash = flash, Effect = effect });
    }

    ///<summary>Toggles one or more lights, from on to off, or, off to on, based on their current state. </summary>
    ///<param name="target">The IEnumerable&lt;ILightEntity&gt; to call this service for</param>
    ///<param name="transition">Duration it takes to get to next state.</param>
    ///<param name="rgbColor">Color for the light in RGB-format. eg: [255, 100, 100]</param>
    ///<param name="colorName">A human readable color name.</param>
    ///<param name="hsColor">Color for the light in hue/sat format. Hue is 0-360 and Sat is 0-100. eg: [300, 70]</param>
    ///<param name="xyColor">Color for the light in XY-format. eg: [0.52, 0.43]</param>
    ///<param name="colorTemp">Color temperature for the light in mireds.</param>
    ///<param name="kelvin">Color temperature for the light in Kelvin.</param>
    ///<param name="brightness">Number indicating brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</param>
    ///<param name="brightnessPct">Number indicating percentage of full brightness, where 0 turns the light off, 1 is the minimum brightness and 100 is the maximum brightness supported by the light.</param>
    ///<param name="profile">Name of a light profile to use. eg: relax</param>
    ///<param name="flash">If the light should flash.</param>
    ///<param name="effect">Light effect.</param>
    public static void Toggle(this IEnumerable<ILightEntityCore> target, long? transition = null, object? rgbColor = null, object? colorName = null, object? hsColor = null, object? xyColor = null, object? colorTemp = null, long? kelvin = null, long? brightness = null, long? brightnessPct = null, string? profile = null, object? flash = null, string? effect = null)
    {
        target.CallService("toggle", new LightToggleParameters { Transition = transition, RgbColor = rgbColor, ColorName = colorName, HsColor = hsColor, XyColor = xyColor, ColorTemp = colorTemp, Kelvin = kelvin, Brightness = brightness, BrightnessPct = brightnessPct, Profile = profile, Flash = flash, Effect = effect });
    }

    ///<summary>Turns off one or more lights.</summary>
    public static void TurnOff(this ILightEntityCore target, LightTurnOffParameters data)
    {
        target.CallService("turn_off", data);
    }

    ///<summary>Turns off one or more lights.</summary>
    public static void TurnOff(this IEnumerable<ILightEntityCore> target, LightTurnOffParameters data)
    {
        target.CallService("turn_off", data);
    }

    ///<summary>Turns off one or more lights.</summary>
    ///<param name="target">The ILightEntity to call this service for</param>
    ///<param name="transition">Duration it takes to get to next state.</param>
    ///<param name="flash">If the light should flash.</param>
    public static void TurnOff(this ILightEntityCore target, long? transition = null, object? flash = null)
    {
        target.CallService("turn_off", new LightTurnOffParameters { Transition = transition, Flash = flash });
    }

    ///<summary>Turns off one or more lights.</summary>
    ///<param name="target">The IEnumerable&lt;ILightEntity&gt; to call this service for</param>
    ///<param name="transition">Duration it takes to get to next state.</param>
    ///<param name="flash">If the light should flash.</param>
    public static void TurnOff(this IEnumerable<ILightEntityCore> target, long? transition = null, object? flash = null)
    {
        target.CallService("turn_off", new LightTurnOffParameters { Transition = transition, Flash = flash });
    }

    ///<summary>Turn on one or more lights and adjust properties of the light, even when they are turned on already. </summary>
    public static void TurnOn(this ILightEntityCore target, LightTurnOnParameters data)
    {
        target.CallService("turn_on", data);
    }

    ///<summary>Turn on one or more lights and adjust properties of the light, even when they are turned on already. </summary>
    public static void TurnOn(this IEnumerable<ILightEntityCore> target, LightTurnOnParameters data)
    {
        target.CallService("turn_on", data);
    }

    ///<summary>Turn on one or more lights and adjust properties of the light, even when they are turned on already. </summary>
    ///<param name="target">The ILightEntity to call this service for</param>
    ///<param name="transition">Duration it takes to get to next state.</param>
    ///<param name="rgbColor">The color for the light (based on RGB - red, green, blue).</param>
    ///<param name="rgbwColor">A list containing four integers between 0 and 255 representing the RGBW (red, green, blue, white) color for the light. eg: [255, 100, 100, 50]</param>
    ///<param name="rgbwwColor">A list containing five integers between 0 and 255 representing the RGBWW (red, green, blue, cold white, warm white) color for the light. eg: [255, 100, 100, 50, 70]</param>
    ///<param name="colorName">A human readable color name.</param>
    ///<param name="hsColor">Color for the light in hue/sat format. Hue is 0-360 and Sat is 0-100. eg: [300, 70]</param>
    ///<param name="xyColor">Color for the light in XY-format. eg: [0.52, 0.43]</param>
    ///<param name="colorTemp">Color temperature for the light in mireds.</param>
    ///<param name="kelvin">Color temperature for the light in Kelvin.</param>
    ///<param name="brightness">Number indicating brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</param>
    ///<param name="brightnessPct">Number indicating percentage of full brightness, where 0 turns the light off, 1 is the minimum brightness and 100 is the maximum brightness supported by the light.</param>
    ///<param name="brightnessStep">Change brightness by an amount.</param>
    ///<param name="brightnessStepPct">Change brightness by a percentage.</param>
    ///<param name="white">Set the light to white mode and change its brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</param>
    ///<param name="profile">Name of a light profile to use. eg: relax</param>
    ///<param name="flash">If the light should flash.</param>
    ///<param name="effect">Light effect.</param>
    public static void TurnOn(this ILightEntityCore target, long? transition = null, object? rgbColor = null, object? rgbwColor = null, object? rgbwwColor = null, object? colorName = null, object? hsColor = null, object? xyColor = null, object? colorTemp = null, long? kelvin = null, long? brightness = null, long? brightnessPct = null, long? brightnessStep = null, long? brightnessStepPct = null, long? white = null, string? profile = null, object? flash = null, string? effect = null)
    {
        target.CallService("turn_on", new LightTurnOnParameters { Transition = transition, RgbColor = rgbColor, RgbwColor = rgbwColor, RgbwwColor = rgbwwColor, ColorName = colorName, HsColor = hsColor, XyColor = xyColor, ColorTemp = colorTemp, Kelvin = kelvin, Brightness = brightness, BrightnessPct = brightnessPct, BrightnessStep = brightnessStep, BrightnessStepPct = brightnessStepPct, White = white, Profile = profile, Flash = flash, Effect = effect });
    }

    ///<summary>Turn on one or more lights and adjust properties of the light, even when they are turned on already. </summary>
    ///<param name="target">The IEnumerable&lt;ILightEntity&gt; to call this service for</param>
    ///<param name="transition">Duration it takes to get to next state.</param>
    ///<param name="rgbColor">The color for the light (based on RGB - red, green, blue).</param>
    ///<param name="rgbwColor">A list containing four integers between 0 and 255 representing the RGBW (red, green, blue, white) color for the light. eg: [255, 100, 100, 50]</param>
    ///<param name="rgbwwColor">A list containing five integers between 0 and 255 representing the RGBWW (red, green, blue, cold white, warm white) color for the light. eg: [255, 100, 100, 50, 70]</param>
    ///<param name="colorName">A human readable color name.</param>
    ///<param name="hsColor">Color for the light in hue/sat format. Hue is 0-360 and Sat is 0-100. eg: [300, 70]</param>
    ///<param name="xyColor">Color for the light in XY-format. eg: [0.52, 0.43]</param>
    ///<param name="colorTemp">Color temperature for the light in mireds.</param>
    ///<param name="kelvin">Color temperature for the light in Kelvin.</param>
    ///<param name="brightness">Number indicating brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</param>
    ///<param name="brightnessPct">Number indicating percentage of full brightness, where 0 turns the light off, 1 is the minimum brightness and 100 is the maximum brightness supported by the light.</param>
    ///<param name="brightnessStep">Change brightness by an amount.</param>
    ///<param name="brightnessStepPct">Change brightness by a percentage.</param>
    ///<param name="white">Set the light to white mode and change its brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</param>
    ///<param name="profile">Name of a light profile to use. eg: relax</param>
    ///<param name="flash">If the light should flash.</param>
    ///<param name="effect">Light effect.</param>
    public static void TurnOn(this IEnumerable<ILightEntityCore> target, long? transition = null, object? rgbColor = null, object? rgbwColor = null, object? rgbwwColor = null, object? colorName = null, object? hsColor = null, object? xyColor = null, object? colorTemp = null, long? kelvin = null, long? brightness = null, long? brightnessPct = null, long? brightnessStep = null, long? brightnessStepPct = null, long? white = null, string? profile = null, object? flash = null, string? effect = null)
    {
        target.CallService("turn_on", new LightTurnOnParameters { Transition = transition, RgbColor = rgbColor, RgbwColor = rgbwColor, RgbwwColor = rgbwwColor, ColorName = colorName, HsColor = hsColor, XyColor = xyColor, ColorTemp = colorTemp, Kelvin = kelvin, Brightness = brightness, BrightnessPct = brightnessPct, BrightnessStep = brightnessStep, BrightnessStepPct = brightnessStepPct, White = white, Profile = profile, Flash = flash, Effect = effect });
    }
}

