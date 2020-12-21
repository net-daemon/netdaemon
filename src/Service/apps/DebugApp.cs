using System.Threading.Tasks;
using System.Linq;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using NetDaemon.Common.Reactive;
using NetDaemon.Common;

namespace Debug
{

    /// <summary> Use this class as startingpoint for debugging </summary>
    public class DebugApp : NetDaemonRxApp
    {

        public override void Initialize()
        {
            RunEvery(TimeSpan.FromSeconds(5), () => Log("Hello world!"));
        }

        [HomeAssistantServiceCall]
        public void CallMeFromHass(dynamic data)
        {
            Log("A call from hass! {data}", data);
        }
    }

}
