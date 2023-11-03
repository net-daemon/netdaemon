using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class HelloAppSrc : IAsyncDisposable
{
    private readonly ILogger<HelloApp> _logger;

    public HelloAppSrc(IHaContext ha, ILogger<HelloApp> logger)
    {
        _logger = logger;
        ha?.Events.Where(n => n.EventType == "test_event").Subscribe( n =>
        {
            logger.LogInformation("Hello testevent");
        });
        ha?.CallService("notify", "persistent_notification", data: new { message = "Notify me", title = "Hello world!" });
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation("disposed app");
        return ValueTask.CompletedTask;
    }
}
