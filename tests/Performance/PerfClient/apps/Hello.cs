using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

#pragma warning disable CA1050

[NetDaemonApp]
public class HelloApp
{
    public HelloApp(IHaContext ha, ILogger<HelloApp> logger)
    {
        logger.LogInformation("Hello from app");
    }
}
