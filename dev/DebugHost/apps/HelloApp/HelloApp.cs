using System;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;

namespace Apps;

[NetDaemonApp]
[Focus]
public class HelloApp
{
    public HelloApp(IHaContext ha, ILogger<HelloApp> logger)
    {
        // .Where(n => n.EventType == "test_event")
        ha?.Events.Where(n => n.EventType == "test_event").Subscribe( n =>
        {
            logger.LogInformation("Hello testevent");
        });
        ha?.CallService("notify", "persistent_notification", data: new { message = "Notify me", title = "Hello world!" });
    }
}