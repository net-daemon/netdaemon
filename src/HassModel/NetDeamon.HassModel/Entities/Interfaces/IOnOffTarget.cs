namespace NetDaemon.HassModel.Entities;



/// <summary>
/// 
/// </summary>
public interface IToggleParameters
{
}

/// <summary>
/// 
/// </summary>
public interface ITurnOffParameters
{
}

public interface ITurnOnParameters
{
}



public interface IOnOffTarget
{   
    // Virtual extension methods (aka default interface methods)

    public void Toggle()
    {
        throw new NotImplementedException();
    }
    
    public void TurnOn()
    {
        throw new NotImplementedException();
    }
    
    public void TurnOff()
    {
        throw new NotImplementedException();
    }
    
    public void Toggle(IEnumerable<IServiceTarget> targets)
    {
        throw new NotImplementedException();
    }
    
    ///<summary>Toggles one or more targets, from on to off, or, off to on, based on their current state. </summary>
    public void Toggle(IToggleParameters data)
    {
        throw new NotImplementedException();
    }

    ///<summary>Toggles one or more targets, from on to off, or, off to on, based on their current state. </summary>
    ///<param name="target">The LightEntity to call this service for</param>
    ///<param name="transition">Duration it takes to get to next state.</param>
    ///<param name="rgbColor">Color for the light in RGB-format. eg: [255, 100, 100]</param>
    ///<param name="colorName">A human readable color name.</param>
    ///<param name="hsColor">Color for the light in hue/sat format. Hue is 0-360 and Sat is 0-100. eg: [300, 70]</param>
    ///<param name="xyColor">Color for the light in XY-format. eg: [0.52, 0.43]</param>
    ///<param name="colorTemp">Color temperature for the light in mireds.</param>
    ///<param name="kelvin">Color temperature for the light in Kelvin.</param>
    ///<param name="brightness">Number indicating brightness, where 0 turns the light off, 1 is the minimum brightness and 255 is the maximum brightness supported by the light.</param>
    ///<param name="brightnessPct">Number indicating percentage of full brightness, where 0 turns the light off, 1 is the minimum brightness and 100 is the maximum brightness supported by the light.</param>
    ///<param name="white">Set the light to white mode.</param>
    ///<param name="profile">Name of a light profile to use. eg: relax</param>
    ///<param name="flash">If the light should flash.</param>
    ///<param name="effect">Light effect.</param>
    public static void Toggle(long? transition = null, object? rgbColor = null, object? colorName = null, object? hsColor = null, object? xyColor = null, object? colorTemp = null, long? kelvin = null, long? brightness = null, long? brightnessPct = null, object? white = null, string? profile = null, object? flash = null, string? effect = null)
    {
        throw new NotImplementedException();
    }
    
    ///<summary>Turns off one or more targets.</summary>
    public static void TurnOff(ITurnOffParameters data)
    {
        throw new NotImplementedException();
    }

    ///<summary>Turns off one or more targets.</summary>
    ///<param name="target">The LightEntity to call this service for</param>
    ///<param name="transition">Duration it takes to get to next state.</param>
    ///<param name="flash">If the light should flash.</param>
    public static void TurnOff(long? transition = null, object? flash = null)
    {
        throw new NotImplementedException();
    }
    
    ///<summary>Turn on one or more targets and adjust properties of the light, even when they are turned on already. </summary>
    public void TurnOn(ITurnOnParameters data)
    {
        throw new NotImplementedException();
    }

    ///<summary>Turn on one or more targets and adjust properties of the light, even when they are turned on already. </summary>
    ///<param name="target">The LightEntity to call this service for</param>
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
    ///<param name="white">Set the light to white mode.</param>
    ///<param name="profile">Name of a light profile to use. eg: relax</param>
    ///<param name="flash">If the light should flash.</param>
    ///<param name="effect">Light effect.</param>
    public static void TurnOn(long? transition = null, object? rgbColor = null, object? rgbwColor = null, object? rgbwwColor = null, object? colorName = null, object? hsColor = null, object? xyColor = null, object? colorTemp = null, long? kelvin = null, long? brightness = null, long? brightnessPct = null, long? brightnessStep = null, long? brightnessStepPct = null, object? white = null, string? profile = null, object? flash = null, string? effect = null)
    {
        throw new NotImplementedException();
    }
}

public interface IOnOffListTarget: IEnumerable<IOnOffTarget>
{
    public void Toggle()
    {
        throw new NotImplementedException();
    }
}