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
        //Entities("binary_sensor.tomas_rum_pir", "binary_sensor.vardagsrum_pir")
        //    .Merge()
        //    .Where(e => e.New.State == "off")
        //    .Subscribe(x =>
        //    {
        //        Log("{entity} ({state})", x.New.EntityId, x.New.State);
        //    });

        
        EventChanges
            .Subscribe(f =>
            {
                Log("event: {domain}.{event} - {data}", f?.Domain??"none", f.Event, f?.Data);
            });
        return Task.CompletedTask;
    }
}


