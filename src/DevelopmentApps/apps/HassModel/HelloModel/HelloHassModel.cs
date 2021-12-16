using NetDaemon.Common;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Common;

namespace NetDaemon.DevelopmentApps.apps.M3Test
{
    [NetDaemonApp]
    public class HelloNewModelApp
    {
        public HelloNewModelApp(IHaContext ha, INetDaemonScheduler scheduler)
        {
            ha.SendEvent("test", new { name="Frank", age = 45});
        }
    }
}