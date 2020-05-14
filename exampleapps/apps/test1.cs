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
public class GlobalApp : NetDaemonApp
{
    // private ISchedulerResult _schedulerResult;
    private int numberOfRuns = 0;
    IDisposable task;
    //public string? SharedThing { get; set; }
    public override Task InitializeAsync()
    {
        Event("TEST_EVENT").Call(async (ev, data) => { Log("EVENT!"); }).Execute();

        // var rand = new Random();
        // var randNumber = rand.Next();
        // RunEvery(TimeSpan.FromSeconds(5), () => Log("Hello world {rand}", randNumber));


        // Entity("sensor.085007098700065859_temperature").StateChanges
        //     .Where(s => s.New.State > 10.0)
        //     .Subscribe(e => Log("Temp: {temp}", e.New.State));
        // RunEvery(TimeSpan.FromSeconds(15), () => throw new Exception("Ohh noo man!"));

        // EventChanges
        //     .Subscribe(f =>
        //     {
        //         Log("event: {domain}.{event} - {data}", f?.Domain??"none", f.Event, f?.Data);
        //     });
        return Task.CompletedTask;
    }
}


