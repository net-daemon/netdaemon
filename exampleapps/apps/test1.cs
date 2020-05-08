using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using System.Linq;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Threading;

// using Netdaemon.Generated.Extensions;
public class GlobalApp : NetDaemonApp
{

    public override async Task InitializeAsync()
    {

        Log("Listening for all events with Rx");
        this.Subscribe(e =>
        {
            Log("Rx - got global event for {0}, state is {1}", e.EntityId, e.State);
        });

        Log("listening for specific entity events with Rx");
        Entity("light.stairs").Subscribe(e =>
        {
            Log("Rx - got specific light.stairs event for {0}: {1}", e.EntityId, e.State);
        });

        Log("listing for 2 entity events with Rx");
        Entity("light.stairs", "light.dining_light").Subscribe(e =>
        {
            Log("Rx - got either stairs or dining event for {0}: {1}", e.EntityId, e.State);
        });
    }
}


