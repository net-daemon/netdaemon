using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Linq;
using System.Runtime.Serialization;
// using Netdaemon.Generated.Extensions;
public class GlobalApp : NetDaemonRxApp
{
    // private ISchedulerResult _schedulerResult;
    private int numberOfRuns = 0;
    IDisposable task;
    //public string? SharedThing { get; set; }
    public override Task InitializeAsync()
    {

        var rand = new Random();
        var randNumber = rand.Next();
        RunEvery(TimeSpan.FromSeconds(5)).Subscribe(s => Log("Hello world {rand}", randNumber));



        // EventChanges
        //     .Subscribe(f =>
        //     {
        //         Log("event: {domain}.{event} - {data}", f?.Domain??"none", f.Event, f?.Data);
        //     });
        return Task.CompletedTask;
    }
}


