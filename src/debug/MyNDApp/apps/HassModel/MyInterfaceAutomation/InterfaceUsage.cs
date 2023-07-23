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
        var myLibraryClass = new MyLibraryClass(entities.Light.LivingRoom, lights, logger);
        for (var i = 0; i < 4; i++)
        {
            myLibraryClass.ToogleTarget();
            Thread.Sleep(1000);
        }
    }
}