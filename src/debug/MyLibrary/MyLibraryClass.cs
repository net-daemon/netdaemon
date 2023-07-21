using NetDaemon.HassModel.Entities;

namespace MyLibrary;

public class MyLibraryClass
{
    public ILightEntity Target;
    public IEnumerable<ILightEntity> TargetList;

    public MyLibraryClass(ILightEntity target, IEnumerable<ILightEntity> lights)
    {
        Target = target;
        TargetList = lights;
    }

    public void ToogleTarget()
    {
        Target.Toggle();
    }
    
    public void ToogleTargetList()
    {
        Target.Toggle(TargetList);
    }
}
