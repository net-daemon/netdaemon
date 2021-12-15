using System;
using NetDaemon.Common;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Integration;

namespace NetDaemon.DevelopmentApps.apps.M3Test
{
    [NetDaemonApp]
    [Focus]
    public class HelloNewModelApp
    {
        public HelloNewModelApp(IHaContext ha, INetDaemonScheduler scheduler)
        {
            ha.RegisterServiceCallBack("CallMe3",  (ServiceData e) => Console.WriteLine($"{e?.name} + {e?.value}"));
        }
    }

    record ServiceData(string? name, int value);

}