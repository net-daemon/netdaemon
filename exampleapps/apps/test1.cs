using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
// using Netdaemon.Generated.Extensions;
public class GlobalApp : NetDaemonApp
{
    // private ISchedulerResult _schedulerResult;
    private int numberOfRuns = 0;

    public string? SharedThing { get; set; }
    public override async Task InitializeAsync()
    {
        SharedThing = "Hello world";
        Log("Logging from global app");
        LogError("OMG SOMETING IS WRONG {error}", "The error!");

        Entities(p =>
            {
                // await Task.Delay(5000);
                Thread.Sleep(10);
                return false;
            }).WhenStateChange("on", "off")
            .Call((a, b, c) => { Log("Logging from global app"); return Task.CompletedTask; })
            .Execute();

        Log("AfterExecute");
        // Entity("light.my_light").TurnOn();
    }
}


