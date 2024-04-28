using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]
public sealed class HelloApp : IAsyncDisposable
{
    private readonly ILogger<HelloApp> _logger;

    public HelloApp(IHaContext ha, ILogger<HelloApp> logger)
    {
        _logger = logger;
        ha.Events.Where(n => n.EventType == "test_event").Subscribe( _ =>
        {
            logger.LogInformation("Hello testevent");
        });
        ha.CallService("notify", "persistent_notification", data: new { message = "Notify me", title = "Hello world!" });
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(5000);
        _logger.LogInformation("disposed app");
        //return ValueTask.CompletedTask;
    }
}
