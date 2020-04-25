using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;

/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class MissingExecuteApp : NetDaemonApp
{
    public override async Task InitializeAsync()
    {
        // Do nothing
        Entity("Test");

        Entity("Test").TurnOn();

        await Entity("jalll").TurnOn()
            .WithAttribute("test", "lslslsl").ExecuteAsync();

        await TestFailInAMethod();

        // Do rest of the needed commands the warning system needs to find
        Event("test").Call((a, b) => Task.CompletedTask);
        Events(n => n.EventId.StartsWith("hello_")).Call((a, b) => Task.CompletedTask);
        InputSelect("i").SetOption("test");
        InputSelects(n => n.EntityId == "lslsls").SetOption("test");
        MediaPlayer("i").Play();
        MediaPlayers(n => n.EntityId == "lslsls").PlayPause();
        Camera("sasdds").Snapshot("asdas");
        Cameras(n => n.EntityId == "lslsls").Snapshot("asdas");
        RunScript("test");
    }

    private async Task TestFailInAMethod()
    {
        await Entity("jalll").TurnOn()
            .WithAttribute("test", "lslslsl").ExecuteAsync();

        Entity("jalll").WhenStateChange(to: "test")
                             .AndNotChangeFor(System.TimeSpan.FromSeconds(1))
                             .Call(async (e, n, o) =>
       {
           Entity("Test").TurnOn();
           var x = new NestedClass(this);
           await x.DoAThing();
       });

    }
}

public class NestedClass
{
    private readonly NetDaemonApp _app;

    public NestedClass(NetDaemonApp app)
    {
        _app = app;
    }

    public async Task DoAThing()
    {
        // Should find this error
        _app.Entities(new string[] { "test.test", "test.test2" }).TurnOn();
    }
}