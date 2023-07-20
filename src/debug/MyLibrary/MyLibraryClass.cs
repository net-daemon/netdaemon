using NetDaemon.HassModel.Entities;

namespace MyLibrary;

public class MyLibraryClass
{
    public IOnOffTarget Target;

    public MyLibraryClass(ILightServiceTarget target)
    {
        Target = target;
    }

    public void ToogleTarget()
    {
        Target.Toggle();
    }
}