using JoySoftware.HomeAssistant.NetDaemon.Common;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace app
{
    public class MyApp : NetDaemonApp
    {
        public override async Task InitializeAsync()
        {
            Log("Listing for events with Rx");

            this.Subscribe(e =>
            {
                Log("Rx - got global event for {0}, state is {1}", e.EntityId, e.State);
            });

            Log("listening for specific entity events with Rx");
            Entity("light.stairs").Subscribe(e =>
            {
                Log("Rx - got specific light.stairs event for {0}: {1}", e.EntityId, e.State);
            });

        }
    }
}