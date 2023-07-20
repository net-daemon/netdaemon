using System.Threading;
using HomeAssistantGenerated;
using MyLibrary;

namespace Debug.apps.HassModel.MyInterfaceAutomation;

[NetDaemonApp]
[Focus]
public class InterfaceUsage
{
    public InterfaceUsage(IHaContext haContext, ILogger<InterfaceUsage> logger)
    {
        var entities = new Entities(haContext);
        var myLibraryClass = new MyLibraryClass(entities.Light.SonoffLed);

        while (true)
        {
            myLibraryClass.ToogleTarget();
            Thread.Sleep(1000);
        }
    }
}