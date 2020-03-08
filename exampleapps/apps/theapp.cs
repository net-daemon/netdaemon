using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using JoySoftware.HomeAssistant.NetDaemon.Common;
/// <summary>
///     This is mainly used to debug real hass use-cases
/// </summary>
public class TheApp : NetDaemonApp
{

    #region -- Config properties --
    public string? StringConfig { get; set; } = null;
    public int? IntConfig { get; set; } = null;
    public IEnumerable<string>? EnumerableConfig { get; set; } = null;

    #endregion

    public static string PrettyPrintDictData(IDictionary<string, object>? dict)
    {

        if (dict == null)
            return string.Empty;

        var builder = new StringBuilder(100);
        foreach (var key in dict.Keys)
        {
            builder.AppendLine($"{key}:{dict[key]}");
        }
        return builder.ToString();
    }
    public override async Task InitializeAsync()
    {
        // await CallService("light", "toggle", new { entity_id = "light.tomas_rum" }, false);

        // var state = GetState("sensor.hacs");
        // state.Attribute.repositoriess = new List<object?>();
        // foreach (IDictionary<string, object?> item in state?.Attribute?.repositoriess as IEnumerable<object?>)
        // {

        //     Log(item["name"] as string);
        // }


        // await Task.Delay(100);
        // Scheduler.RunEvery(TimeSpan.FromSeconds(10), async () =>
        // {
        //     // dynamic x = new ExpandoObject();

        // });

        // ListenServiceCall("scene", "turn_on", async (data) =>
        // {
        //     Log("Service call!");
        //     var prettyData = PrettyPrintDictData(data as IDictionary<string, object>);
        //     Log(prettyData);


        // });

        // var time = "07:24:23";
        // // Do nothing
        // Schedule("14:41:01");
        // Schedule("14:41:02");
        // Schedule("14:41:03");
        // Schedule("14:41:04");
        // Schedule("14:41:05");
        // Schedule("14:41:06");
        // Schedule("14:41:07");
        // Schedule("14:41:08");
        // Schedule("14:41:09");
        // Schedule("14:41:10");
        // Schedule("14:41:11");
        // Schedule("14:41:12");
        // Schedule("14:41:13");
        // Schedule("14:41:14");
        // Scheduler.RunDaily(time, new DayOfWeek[]
        // {
        //     DayOfWeek.Sunday,
        // }, async () => Log($"This is correct time! {DateTime.Now}"));
        // Scheduler.RunEveryMinute(30, async () => Log($"{DateTime.Now}"));
        // return Task.CompletedTask;
    }

    private void Schedule(string time)
    {

        Scheduler.RunDaily(time, new DayOfWeek[]
      {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday,
      }, async () => Log($"Time! {time}, {DateTime.Now}"));

    }

    // [HomeAssistantServiceCall]
    // public async Task CallMeFromHass(dynamic data)
    // {
    //     Log("A call from hass!");
    // }
}