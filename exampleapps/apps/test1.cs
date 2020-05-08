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
        Log("Listening for events with Rx");
        this.ForEach(e =>
        {
            Log("Rx - got event for {0}, state is {1}", e.EntityId, e.State);
        });

    }
}


