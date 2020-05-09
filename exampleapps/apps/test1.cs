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

    //public string? SharedThing { get; set; }
    public override async Task InitializeAsync()
    {
        //Log("Rx - TEST");
        //States.Subscribe(t => { Log("{entity}: {state}({oldstate})", t.New.EntityId, t.New.State, t.Old.State); });

        //Log("Rx - TEST");
        //States.Where(t => t.New.EntityId.StartsWith("sensor.") && t.New.State != t.Old.State)
        //    .Subscribe(t => { Log("{entity}: {state}({oldstate})", t.New.EntityId, t.New.State, t.Old.State); });

        //Observable.Interval(TimeSpan.FromSeconds(10)).Subscribe(e =>
        //{
        //    Entity("light.tomas_rum").Toggle();
        //    var x = State("light.tomas_rum");
        //    Log("The light in Tomas room is : {state}", x?.State);

        Log("Rx - TEST");
        StateChanges.Subscribe(t => 
        { 
            Log("{entity}: {state}({oldstate})", t.New.EntityId, t.New.State, t.Old.State);
            Thread.Sleep(1000);
            Log("Slept 1000 ms");
        });
    //});
        //States.Subscribe(tuple =>
        //{
        //    var (o, n) = tuple;
        //    Log("{entity}: {state}({oldstate})", n.EntityId, n.State, o.State);
        //    Thread.Sleep(1000);
        //    Log("Slept 1000 ms");
        //});

        //this.Where( n => n.EntityId.StartsWith("light.")).Subscribe(x => Log(x.EntityId));
        //State.Where(x => x.EntityId.StartsWith("light.")).Subscribe(e =>
        //{
        //    Log("Rx - got global event for {0}, state is {1}", e.EntityId, e.State);
        //});

        //SharedThing = "Hello world";
        //Log("Logging from global app");
        //LogError("OMG SOMETING IS WRONG {error}", "The error!");

        //Entities(p =>
        //    {
        //        // await Task.Delay(5000);
        //        Thread.Sleep(10);
        //        return false;
        //    }).WhenStateChange("on", "off")
        //    .Call((a, b, c) => { Log("Logging from global app"); return Task.CompletedTask; })
        //    .Execute();

        //Log("AfterExecute");
        //// Entity("light.my_light").TurnOn();
    }
}


