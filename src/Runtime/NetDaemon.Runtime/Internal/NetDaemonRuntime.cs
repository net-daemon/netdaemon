using System.Reactive.Linq;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace NetDaemon.Runtime.Internal;

internal class NetDaemonRuntime : IRuntime
{
    private const string Version = "custom_compiled";
    private const int TimeoutInSeconds = 30;
    private readonly ICacheManager _cacheManager;

    private readonly HomeAssistantSettings _haSettings;
    private readonly IHomeAssistantRunner _homeAssistantRunner;
    private readonly IOptions<AppConfigurationLocationSetting> _locationSettings;

    private readonly ILogger<NetDaemonRuntime> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IAppModelContext? _applicationModelContext;
    private CancellationToken? _stoppingToken;
    private CancellationTokenSource? _runnerCancelationSource;

    public bool IsConnected;

    public NetDaemonRuntime(
        IHomeAssistantRunner homeAssistantRunner,
        IOptions<HomeAssistantSettings> settings,
        IOptions<AppConfigurationLocationSetting> locationSettings,
        IServiceProvider serviceProvider,
        ILogger<NetDaemonRuntime> logger,
        ICacheManager cacheManager)
    {
        _haSettings = settings.Value;
        _homeAssistantRunner = homeAssistantRunner;
        _locationSettings = locationSettings;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cacheManager = cacheManager;
    }

    // These internals are used primarily for testing purposes
    internal IReadOnlyCollection<IApplication> ApplicationInstances =>
        _applicationModelContext?.Applications ?? Array.Empty<IApplication>();

    private readonly TaskCompletionSource _startedAndConnected = new();

    private Task _runnerTask = Task.CompletedTask;

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting NetDaemon runtime version {Version}.");

        _stoppingToken = stoppingToken;

        _homeAssistantRunner.OnConnect
            .Select(async c => await OnHomeAssistantClientConnected(c, stoppingToken).ConfigureAwait(false))
            .Subscribe();
        _homeAssistantRunner.OnDisconnect
            .Select(async s => await OnHomeAssistantClientDisconnected(s).ConfigureAwait(false))
            .Subscribe();
        try
        {
            _runnerCancelationSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            _runnerTask = _homeAssistantRunner.RunAsync(
                _haSettings.Host,
                _haSettings.Port,
                _haSettings.Ssl,
                _haSettings.Token,
                _haSettings.WebsocketPath,
                TimeSpan.FromSeconds(TimeoutInSeconds),
                _runnerCancelationSource.Token);

            await _startedAndConnected.Task;
        }
        catch (OperationCanceledException)
        {
            // Ignore and just stop
        }
    }

    private async Task OnHomeAssistantClientConnected(
        IHomeAssistantConnection haConnection,
        CancellationToken cancelToken)
    {
        _logger.LogInformation("Successfully connected to Home Assistant");
        IsConnected = true;

        await _cacheManager.InitializeAsync(cancelToken).ConfigureAwait(false);

        await LoadNewAppContextAsync(haConnection, cancelToken);

        _startedAndConnected.SetResult();
    }

    private async Task LoadNewAppContextAsync(IHomeAssistantConnection haConnection, CancellationToken cancelToken)
    {
        var appModel = _serviceProvider.GetService<IAppModel>();
        if (appModel == null) return;

        try
        {
            // this logging is a bit weird in this class
            if (!string.IsNullOrEmpty(_locationSettings.Value.ApplicationConfigurationFolder))
                _logger.LogDebug("Loading applications from folder {Path}",
                    Path.GetFullPath(_locationSettings.Value.ApplicationConfigurationFolder));
            else
                _logger.LogDebug("Loading applications with no configuration folder");
            
            _applicationModelContext = await appModel.LoadNewApplicationContext(CancellationToken.None).ConfigureAwait(false);

            // Handle state change for apps if registered
            var appStateHandler = _serviceProvider.GetService<IHandleHomeAssistantAppStateUpdates>();
            if (appStateHandler == null) return;
            
            await appStateHandler.InitializeAsync(haConnection, _applicationModelContext);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "   Failed to initialize apps");
            throw;
        }
    }

    private async Task OnHomeAssistantClientDisconnected(DisconnectReason reason)
    {
        if (_stoppingToken?.IsCancellationRequested == true || reason == DisconnectReason.Client)
        {
            _logger.LogInformation("HassClient disconnected cause of user stopping");
        }
        else
        {
            var reasonString = reason switch
            {
                DisconnectReason.Remote => "home assistant closed the connection",
                DisconnectReason.Error => "unknown error, set loglevel to debug to view details",
                DisconnectReason.Unauthorized => "token not authorized",
                DisconnectReason.NotReady => "home assistant not ready yet",
                _ => "unknown error"
            };
            _logger.LogInformation("Home Assistant disconnected due to {Reason}, connect retry in {TimeoutInSeconds} seconds",
                reasonString, TimeoutInSeconds);
        }

        await DisposeApplicationsAsync().ConfigureAwait(false);
        IsConnected = false;
    }

    private async Task DisposeApplicationsAsync()
    {
        if (_applicationModelContext is not null)
        {
            foreach (var applicationInstance in _applicationModelContext.Applications)
                await applicationInstance.DisposeAsync().ConfigureAwait(false);
            
            _applicationModelContext = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeApplicationsAsync().ConfigureAwait(false);
        _runnerCancelationSource?.Cancel();
        try
        {
            await _runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
    }
}