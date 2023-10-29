using NetDaemon.HassModel.Entities;


namespace MyLibrary;

public class MyLibraryClass
{
    public LightEntity Target { get; set; }
    public IEnumerable<ILightEntityCore> TargetList { get; set; }

    public MyLibraryClass(ILightEntityCore target, IEnumerable<ILightEntityCore> lights)
    {
        Target = new LightEntity(target);
        TargetList = lights;
    }

    public void ToggleTarget()
    {
        Target.Toggle();
    }


    public void Increment()
    {
        var current = (long)(Target.Attributes?.Brightness ?? 0);
        var newBrightness = current + 10;
        if (newBrightness <= 256)
        {
            Target.TurnOn(brightness: newBrightness);
        }
        else
        {
            Target.TurnOff();
        }
    }


    public void ToggleTargetList()
    {
        TargetList.Toggle();
    }


    public void HalfBrightness()
    {
        TargetList.TurnOn(brightnessPct: 50);
    }

}
