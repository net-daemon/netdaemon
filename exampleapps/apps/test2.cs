using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;
using System.Linq;
using System;
using System.Reactive.Linq;
using System.Collections.Generic;
// using Netdaemon.Generated.Extensions;
public class BatteryManager : NetDaemonRxApp
{
    // private ISchedulerResult _schedulerResult;
    private int numberOfRuns = 0;

    public string? HelloWorldSecret { get; set; }
    public override async Task InitializeAsync()
    {
        Log("Hello");
        Log("Hello {name}", "Tomas");
        // RunEvery(TimeSpan.FromSeconds(5), () => Log("Hello world!"));
        // RunDaily("13:00:00", () => Log("Hello world!"));
        // RunIn(TimeSpan.FromSeconds(5), () => Entity("light.tomas_rum").TurnOn());
        // Entity("light.tomas_rum")
        //     .StateChanges
        //         .Subscribe(s => Log("Chanche {entity}", s.New.State));

        // StateChanges
        //     .Where(e => e.New.EntityId.StartsWith("light."))
        //     .Subscribe(e =>
        //        {
        //            Log("Hello!");
        //        });
        // EventChanges
        //        //   .Where(e => e.Domain == "scene" && e.Event == "turn_on")
        //        .Subscribe(e =>
        //        {
        //            Log("Hello!");
        //        },
        //        err => LogError(err, "Ohh nooo!"),
        //        () => Log("Ending event"));



        // Event("TEST_EVENT").Call(async (ev, data) => { Log("EVENT2!"); }).Execute();

        // Scheduler.RunEvery(5000, () => { var x = 0; var z = 4 / x; return Task.CompletedTask; });
        // Entity("sun.sun").WhenStateChange(allChanges: true).Call((entityid, to, from) => throw new Exception("Test")).Execute();
        // var app = (GlobalApp)GetApp("global_app");

        // Log($"The global app shows {app.SharedThing}");
        // int? test = null;
        // bool testa = test.HasValue;

        // await this.LightEx().JulbelysningVardagsrumH.TurnOn().ExecuteAsync();
        // await this.MediaPlayerEx().PlexChromecast.Play().ExecuteAsync();
        // Scheduler.RunIn(TimeSpan.FromSeconds(10), async () => await DoTheMagic("s").ConfigureAwait(false));

    }

    // async Task DoTheMagic()
    // {
    //     Log("WAITING FOR STATE");
    //     // await Entity("binary_sensor.vardagsrum_pir").DelayUntilStateChange(to: "on").Task;

    //     var task = DelayUntilStateChange(new string[] { "binary_sensor.vardagsrum_pir" }, to: "on", from: "off");

    //     await task.Task;
    //     Log("STATE IS COOL DAMN IT!!!");

    // }
    // async Task DoTheMagic(string test)
    // {
    //     Log("WAITING FOR STATE");
    //     // await Entity("binary_sensor.vardagsrum_pir").DelayUntilStateChange(to: "on").Task;

    //     var task = DelayUntilStateChange(new string[] { "binary_sensor.vardagsrum_pir" }, to: "on", from: "off");

    //     await task.Task;
    //     Log("STATE IS COOL DAMN IT!!!");

    // }
    // bool doingWork = false;
    // private async Task DoWork()
    // {
    //     if (doingWork)
    //         return;

    //     // Time to do work
    //     Entity(Wh)
    // }

    // private async Task MyMotionSensorStateChange(string entityId, EntityState? newState, EntityState? oldState)
    // {
    //     await Entity("light.light1").TurnOn().ExecuteAsync();
    // }

    // [HomeAssistantServiceCall]
    // public async Task CallMeFromHass(dynamic data)
    // {
    //     Log("A call from hass!");
    // }
}

// public static class NotifyExtensions
// {
//     public static async Task NotifyMobile(this NetDaemonApp app, string mobileId, string title, string message)
//     {
//         var data = new Obj
//         {
//             ["title"] = title,
//             ["message"] = message
//         };
//         await app.CallService("notify", mobileId, data, false);
//     }

//     public static async Task NotifyMobileThread(this NetDaemonApp app, string mobileId, string title, string message, string threadId)
//     {
//         var data = new Obj
//         {
//             ["title"] = title,
//             ["message"] = message,
//             ["data"] = new Obj
//             {
//                 ["push"] = new Obj
//                 {
//                     ["thread-id"] = threadId
//                 }
//             }
//         };
//         await app.CallService("notify", mobileId, data, false);
//     }
// }
