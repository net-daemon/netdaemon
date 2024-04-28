using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using HomeAssistantGenerated;
using Microsoft.Extensions.Logging;
using MyLibrary;
using NetDaemon.AppModel;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace Debug.apps.HassModel.MyInterfaceAutomation;

[NetDaemonApp]
//[Focus]
public class InterfaceUsage
{
    public InterfaceUsage(IHaContext haContext, ILogger<InterfaceUsage> logger, IScheduler scheduler)
    {
        var entities = new TestEntities(haContext);
        IEnumerable<ILightEntityCore> lights = new[] { entities.Light.Zolder, entities.Light.Woonkamer };


        // pass generated types to the library using the commonly known interfaces
        var myLibraryClass = new MyLibraryClass(entities.Light.Zolder, lights);

        scheduler.SchedulePeriodic(TimeSpan.FromSeconds(1), () => myLibraryClass.Increment());
    }
}
