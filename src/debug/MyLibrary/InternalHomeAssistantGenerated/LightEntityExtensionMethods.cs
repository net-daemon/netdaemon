using NetDaemon.HassModel.Entities;

namespace MyLibrary;

public static class LightEntityExtensionMethods
{
    public static void Toggle(this ILightEntity target, IToggleParameters data)
    {   
        Console.WriteLine("Toggle");
        target.CallService("toggle", data);
    }

    public static void Toggle(this IEnumerable<ILightEntity> target, IToggleParameters data)
    {
        target.CallService("toggle", data);
    }
    
    ///<summary>Turns off one or more lights.</summary>
    public static void TurnOff(this ILightEntity target, ITurnOffParameters data)
    {
        target.CallService("turn_off", data);
    }

    ///<summary>Turns off one or more lights.</summary>
    public static void TurnOff(this IEnumerable<ILightEntity> target, ITurnOffParameters data)
    {
        target.CallService("turn_off", data);
    }

    ///<summary>Turn on one or more lights and adjust properties of the light, even when they are turned on already. </summary>
    public static void TurnOn(this ILightEntity target, ITurnOnParameters data)
    {
        target.CallService("turn_on", data);
    }

    ///<summary>Turn on one or more lights and adjust properties of the light, even when they are turned on already. </summary>
    public static void TurnOn(this IEnumerable<ILightEntity> target, ITurnOnParameters data)
    {
        target.CallService("turn_on", data);
    }
}