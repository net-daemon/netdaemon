using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class ClientApp : IAsyncDisposable
{
    private readonly ILogger<HelloApp> _logger;

    public ClientApp(ILogger<HelloApp> logger, ITriggerManager triggerManager)
    {
        _logger = logger;

        var triggerObservable = triggerManager.RegisterTrigger(
        new
        {
            platform = "state",
            entity_id = new string[] { "media_player.vardagsrum" },
            from = new string[] { "idle", "playing" },
            to = "off"
        });

        triggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
        );
                
        var timePatternTriggerObservable = triggerManager.RegisterTrigger<TimePatternResult>(new
        {
            platform = "time_pattern",
            id = "some id",
            seconds = "/1"
        });
        
        var disposedSubscription = timePatternTriggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
        );
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation("disposed app");
        return ValueTask.CompletedTask;
    }
    
    record TimePatternResult(string id, string alias, string platform, DateTimeOffset now, string description);
}


