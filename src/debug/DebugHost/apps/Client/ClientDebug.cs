using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Internal;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class ClientApp : IAsyncInitializable, IAsyncDisposable
{
    private readonly ILogger<HelloApp> _logger;
    private readonly ITriggerManager _triggerManager;
    public ClientApp(ILogger<HelloApp> logger, ITriggerManager triggerManager)
    {
        _logger = logger;
        _triggerManager = triggerManager;
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation("disposed app");
        return ValueTask.CompletedTask;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var triggerObservable = await _triggerManager.RegisterTrigger(new StateTrigger()
        {
            EntityId = new string[] { "media_player.vardagsrum" },
            Attribute = new []{"volume_level"}
            // From = new string[] { "on" },
            // To = new string[] {"off"}
        });

        triggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
            );
                
        var timePatternTriggerObservable = await _triggerManager.RegisterTrigger(new TimePatternTrigger()
        {
            Seconds = "/1"
        });
        
        var disposedSubscription = timePatternTriggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
            );
    }
}