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
// [Focus]
public sealed class ClientApp : IAsyncInitializable, IAsyncDisposable
{
    private readonly ILogger<HelloApp> _logger;
    private IObservable<HassMessage>? _triggerObservable;
    private IObservable<HassMessage>? _timePatternTriggerObservable;
    private readonly IHomeAssistantRunner _haConnectionRunner;
    private readonly IHomeAssistantConnection _haConnection;
    private IDisposable _disposedSubscription;
    public ClientApp(IHomeAssistantRunner haConnectionRunner, ILogger<HelloApp> logger)
    {
        _haConnectionRunner = haConnectionRunner;
        _haConnection = haConnectionRunner.CurrentConnection ?? throw new Exception("Fail");
        _logger = logger;
        
    }

    public ValueTask DisposeAsync()
    {
        _logger.LogInformation("disposed app");
        _disposedSubscription.Dispose();
        return ValueTask.CompletedTask;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _triggerObservable = await _haConnection.SubscribeToTriggerAsync(new StateTrigger()
        {
            EntityId = new string[] { "media_player.vardagsrum" },
            Attribute = "volume_level"
            // From = new string[] { "on" },
            // To = new string[] {"off"}
        }, cancellationToken);

        _triggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
            );
                
        _timePatternTriggerObservable = await _haConnection.SubscribeToTriggerAsync(new TimePatternTrigger()
        {
            Seconds = "/5"
        }, cancellationToken);
        
        _disposedSubscription = _timePatternTriggerObservable.Subscribe(n => 
            _logger.LogCritical("Got trigger message: {Message}", n)
            );
        
        
    }
}