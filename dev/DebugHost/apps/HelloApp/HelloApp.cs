using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]
// [Focus]
public class HelloApp : IAsyncDisposable
{
    public HelloApp(IHaContext ha, ILogger<HelloApp> logger)
    {
        // // .Where(n => n.EventType == "test_event")
        // ha?.Events.Where(n => n.EventType == "test_event").Subscribe( n =>
        // {
        //     logger.LogInformation("Hello testevent");
        // });
        // ha?.CallService("notify", "persistent_notification", data: new { message = "Notify me", title = "Hello world!" });
    }

    public ValueTask DisposeAsync()
    {
        Console.WriteLine("disposed app");
        return ValueTask.CompletedTask;
    }
}