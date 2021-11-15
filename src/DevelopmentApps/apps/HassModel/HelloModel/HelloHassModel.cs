using System;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.DevelopmentApps.Domains.Climate;
using NetDaemon.DevelopmentApps.Domains.Sensor;
using NetDaemon.DevelopmentApps.Domains.Zone;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.DevelopmentApps.apps.M3Test
{
    [NetDaemonApp]
    // [Focus]
    public class HelloNewModelApp
    {
        public HelloNewModelApp(IHaContext ha, INetDaemonScheduler scheduler)
        {
            // ha.CallService("notify", "persistent_notification", data: new { message = "it is a test app", title = "Hello HassModel!!" });
        }
    }

}