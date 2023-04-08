using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Client;
using NetDaemon.Client.Common.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.HassModel;

namespace Apps;

[NetDaemonApp]
[Focus]
public sealed class ClientApp : IAsyncDisposable, IAsyncInitializable
{
    private readonly ILogger<HelloApp> _logger;
    private readonly IHomeAssistantConnection _haConn;
    private IObservable<HassMessage>? _triggerObservable;
    private IObservable<HassMessage>? _timePatternTriggerObservable;

    public ClientApp(IHomeAssistantConnection haConn, ILogger<HelloApp> logger)
    {
        _haConn = haConn;
        
        _logger = logger;
        
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation("disposed app");
        return ValueTask.CompletedTask;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _triggerObservable = await _haConn.SubscribeToTriggerAsync(new StateTrigger()
        {
            EntityId = new string[] { "input_boolean.baaaanan" },
            From = new string[] { "on" },
            To = new string[] {"off"}
        }, cancellationToken);

        _triggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
            );
                
        _timePatternTriggerObservable = await _haConn.SubscribeToTriggerAsync(new TimePatternTrigger()
        {
            Seconds = "/5"
        }, cancellationToken);

        _timePatternTriggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
            );
        
        
    }
}