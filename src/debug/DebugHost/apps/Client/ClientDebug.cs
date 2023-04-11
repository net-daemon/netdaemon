using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class ClientApp : IAsyncDisposable
{
    private readonly ILogger<HelloApp> _logger;
    private readonly ITriggerManager _triggerManager;
    public ClientApp(ILogger<HelloApp> logger, ITriggerManager triggerManager)
    {
        _logger = logger;
        _triggerManager = triggerManager;
        
        var triggerObservable = _triggerManager.RegisterTrigger(new StateTrigger()
        {
            EntityId = new string[] { "media_player.vardagsrum" },
            Attribute = "volume_level"
            // From = new string[] { "on" },
            // To = new string[] {"off"}
        });

        triggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
        );
                
        var timePatternTriggerObservable = _triggerManager.RegisterTrigger(new
        {
            platform = "time_pattern",
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
}