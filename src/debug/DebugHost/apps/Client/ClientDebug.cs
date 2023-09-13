using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class ClientApp 
{
    private readonly ILogger<HelloApp> _logger;

    public ClientApp(IAppModelContext ctx, IScheduler scheduler)
    {
        var apps = ctx.Applications.ToList();
        scheduler.Schedule(TimeSpan.FromSeconds(2), () => apps[1].DisableAsync().Wait());
    }
}

[NetDaemonApp]
[Focus]
public sealed class ClientApp2 : IAsyncDisposable
{
    private readonly ILogger<HelloApp> _logger;

    public ClientApp2(IAppModelContext ctx)
    {
        var apps = ctx.Applications.ToList();
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation("disposed");
        return ValueTask.CompletedTask;
    }
}


