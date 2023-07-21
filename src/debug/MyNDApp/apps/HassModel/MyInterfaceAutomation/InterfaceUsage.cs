using System.Collections.Generic;
using System.Threading;
using HomeAssistantGenerated;
using MyLibrary;
using NetDaemon.HassModel.Entities;

namespace Debug.apps.HassModel.MyInterfaceAutomation;

[NetDaemonApp]
[Focus]
public class InterfaceUsage
{
    public InterfaceUsage(IHaContext haContext, ILogger<InterfaceUsage> logger)
    {
        var entities = new Entities(haContext);
        IEnumerable<ILightEntity> lights = new[] { entities.Light.LivingRoom, entities.Light.SonoffLed };
        var myLibraryClass = new MyLibraryClass(entities.Light.SonoffLed, lights);
        while (true)
        {
            myLibraryClass.ToogleTargetList();
            Thread.Sleep(1000);
        }
    }
}