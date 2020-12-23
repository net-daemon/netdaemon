using System;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;

namespace NetDaemon.DevelopmentApps.apps.DebugApp
{

    /// <summary> Use this class as startingpoint for debugging </summary>
    public class DebugApp : NetDaemonRxApp
    {

        public override void Initialize()
        {
            RunEvery(TimeSpan.FromSeconds(5), () => Log("Hello developer!"));
        }

        [HomeAssistantServiceCall]
        public void CallMeFromHass(dynamic data)
        {
            Log("A call from hass! {data}", data);
        }
    }

}
