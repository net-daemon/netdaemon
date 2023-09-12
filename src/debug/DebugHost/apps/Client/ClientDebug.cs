using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]

public sealed class ClientApp : IAsyncDisposable
{
    private readonly ILogger<HelloApp> _logger;

    public ClientApp(IAppModelContext ctx)
    {
        var apps = ctx.Applications.ToList();
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation("disposed app");
        return ValueTask.CompletedTask;
    }
    
    record TimePatternResult(string id, string alias, string platform, DateTimeOffset now, string description);
}


