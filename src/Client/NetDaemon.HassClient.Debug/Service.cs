using System.Reactive.Linq;

namespace NetDaemon.HassClient.Debug;

internal class DebugService : BackgroundService
{
    private const int TimeoutInSeconds = 5;

    private readonly HomeAssistantSettings _haSettings;
    private readonly IHomeAssistantRunner _homeAssistantRunner;

    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly ILogger<DebugService> _logger;

    private CancellationToken? _cancelToken;
    private IHomeAssistantConnection? _connection;

    public DebugService(
        IHostApplicationLifetime hostLifetime,
        IHomeAssistantRunner homeAssistantRunner,
        IOptions<HomeAssistantSettings> settings,
        ILogger<DebugService> logger)
    {
        _haSettings = settings.Value;
        _hostLifetime = hostLifetime;
        _homeAssistantRunner = homeAssistantRunner;
        _logger = logger;

        homeAssistantRunner.OnConnect
            .Select(async s => await OnHomeAssistantClientConnected(s).ConfigureAwait(false))
            .Subscribe();
        homeAssistantRunner.OnDisconnect.Subscribe(OnHomeAssistantClientDisconnected);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancelToken = stoppingToken;
        await _homeAssistantRunner.RunAsync(
            _haSettings.Host,
            _haSettings.Port,
            _haSettings.Ssl,
            _haSettings.Token,
            TimeSpan.FromSeconds(TimeoutInSeconds),
            stoppingToken).ConfigureAwait(false);

        // Stop application if this is exited and use _cancelToken as token
        _hostLifetime.StopApplication();
    }

    private async Task OnHomeAssistantClientConnected(IHomeAssistantConnection connection)
    {
        _logger.LogInformation("HassClient connected and processing events");
        var hassEvents = await connection.SubscribeToHomeAssistantEventsAsync(null, _cancelToken ?? CancellationToken.None).ConfigureAwait(false);
        hassEvents.Subscribe(HandleEvent);
      
    }

    private void OnHomeAssistantClientDisconnected(DisconnectReason reason)
    {
        _logger.LogInformation("HassClient disconnected cause of {Reason}, connect retry in {Timeout} seconds",
            TimeoutInSeconds, reason);
        // Here you would typically cancel and dispose any functions  
        // using the connection
        if (_connection is not null) _connection = null;
    }

    private void HandleEvent(HassEvent hassEvent)
    {
        _logger.LogDebug("New event ({EventType})", hassEvent.EventType);
        switch (hassEvent.EventType)
        {
            case "state_changed":
                var state = hassEvent.ToStateChangedEvent();
                _logger.LogInformation("state changed: {State}", state);
                break;
        }
    }
}