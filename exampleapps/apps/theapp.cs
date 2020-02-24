using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class TheApp : NetDaemonApp
{

    #region -- Config properties --
    public string? StringConfig { get; set; } = null;
    public int? IntConfig { get; set; } = null;
    public IEnumerable<string>? EnumerableConfig { get; set; } = null;

    #endregion

    public override Task InitializeAsync()
    {

        var time = "07:24:23";
        // // Do nothing
        Scheduler.RunDaily(time, new DayOfWeek[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday,
        }, async () => Log($"Time! {DateTime.Now}"));

        // Scheduler.RunDaily(time, new DayOfWeek[]
        // {
        //     DayOfWeek.Sunday,
        // }, async () => Log($"This is correct time! {DateTime.Now}"));
        // Scheduler.RunEveryMinute(30, async () => Log($"{DateTime.Now}"));
        return Task.CompletedTask;
    }

    // [HomeAssistantServiceCall]
    // public async Task CallMeFromHass(dynamic data)
    // {
    //     Log("A call from hass!");
    // }
}