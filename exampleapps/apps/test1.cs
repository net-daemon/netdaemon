using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using System.Linq;
using System;
using System.Collections.Generic;
// using Netdaemon.Generated.Extensions;
public class GlobalApp : NetDaemonApp
{
    // private ISchedulerResult _schedulerResult;
    private int numberOfRuns = 0;

    public string? SharedThing { get; set; }
    public override async Task InitializeAsync()
    {
        SharedThing = "Hello world";

    }
}


