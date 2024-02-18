using System.Reactive.Linq;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace NetDaemon.Runtime.Internal;

internal class NetDaemonRuntime(IHomeAssistantRunner homeAssistantRunner,
        IOptions<HomeAssistantSettings> settings,
        IOptions<AppConfigurationLocationSetting> locationSettings,
        IServiceProvider serviceProvider,
        ILogger<NetDaemonRuntime> logger,
        ICacheManager cacheManager)
    : IRuntime
{
    private const string Version = "custom_compiled";
    private const int TimeoutInSeconds = 30;

    private readonly HomeAssistantSettings _haSettings = settings.Value;

    private IAppModelContext? _applicationModelContext;
    private CancellationToken? _stoppingToken;
    private CancellationTokenSource? _runnerCancellationSource;

    public bool IsConnected;

    // These internals are used primarily for testing purposes
    internal IReadOnlyCollection<IApplication> ApplicationInstances =>
        _applicationModelContext?.Applications ?? Array.Empty<IApplication>();

    private readonly TaskCompletionSource _startedAndConnected = new();

    private Task _runnerTask = Task.CompletedTask;

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Starting NetDaemon runtime version {Version}.");

        _stoppingToken = stoppingToken;

        homeAssistantRunner.OnConnect
            .Select(async c => await OnHomeAssistantClientConnected(c, stoppingToken).ConfigureAwait(false))
            .Subscribe();
        homeAssistantRunner.OnDisconnect
            .Select(async s => await OnHomeAssistantClientDisconnected(s).ConfigureAwait(false))
            .Subscribe();
        try
        {
            _runnerCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

            _runnerTask = homeAssistantRunner.RunAsync(
                _haSettings.Host,
                _haSettings.Port,
                _haSettings.Ssl,
                _haSettings.Token,
                _haSettings.WebsocketPath,
                TimeSpan.FromSeconds(TimeoutInSeconds),
                _runnerCancellationSource.Token);

            // Make sure we only return after the connection is made and initialization is ready
            await Task.WhenAny( _startedAndConnected.Task, _runnerTask);
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
        try
        {
            logger.LogInformation("Successfully connected to Home Assistant");

            if (_applicationModelContext is not null)
            {
                // Something wrong with unloading and disposing apps on restart of HA, we need to prevent apps loading multiple times
                logger.LogWarning("Applications were not successfully disposed during restart, skippin loading apps again");
                return;
            }

            IsConnected = true;

            await cacheManager.InitializeAsync(cancelToken).ConfigureAwait(false);

            await LoadNewAppContextAsync(haConnection, cancelToken);

            _startedAndConnected.TrySetResult();
        }
        catch (Exception ex)
        {
            if (!_startedAndConnected.Task.IsCompleted)
            {
                // This means this was the first time we connected and StartAsync is still awaiting _startedAndConnected
                // By setting the exception on the task it will propagate up.
                _startedAndConnected.SetException(ex);
            }
            else
            {
                // There is none waiting for this event handler to finish so we need to Log the exception here
                logger.LogCritical(ex, "Error re-initializing after reconnect to Home Assistant");
            }
        }
    }

    private async Task LoadNewAppContextAsync(IHomeAssistantConnection haConnection, CancellationToken cancelToken)
    {
        var appModel = serviceProvider.GetService<IAppModel>();
        if (appModel == null) return;

        // this logging is a bit weird in this class
        if (!string.IsNullOrEmpty(locationSettings.Value.ApplicationConfigurationFolder))
            logger.LogDebug("Loading applications from folder {Path}",
                Path.GetFullPath(locationSettings.Value.ApplicationConfigurationFolder));
        else
            logger.LogDebug("Loading applications with no configuration folder");

        _applicationModelContext = await appModel.LoadNewApplicationContext(CancellationToken.None).ConfigureAwait(false);

        // Handle state change for apps if registered
        var appStateHandler = serviceProvider.GetService<IHandleHomeAssistantAppStateUpdates>();
        if (appStateHandler == null) return;

        await appStateHandler.InitializeAsync(haConnection, _applicationModelContext);
    }

    private async Task OnHomeAssistantClientDisconnected(DisconnectReason reason)
    {
        if (_stoppingToken?.IsCancellationRequested == true || reason == DisconnectReason.Client)
        {
            logger.LogInformation("HassClient disconnected cause of user stopping");
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
            logger.LogInformation("Home Assistant disconnected due to {Reason}, connect retry in {TimeoutInSeconds} seconds",
                reasonString, TimeoutInSeconds);
        }

        try
        {
            await DisposeApplicationsAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error disposing applications");
        }
        IsConnected = false;
    }

    private async Task DisposeApplicationsAsync()
    {
        if (_applicationModelContext is not null)
        {
            await _applicationModelContext.DisposeAsync();

            _applicationModelContext = null;
        }
    }

    private bool _isDisposed;
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        await DisposeApplicationsAsync().ConfigureAwait(false);
        if (_runnerCancellationSource is not null)
            await _runnerCancellationSource.CancelAsync();
        try
        {
            await _runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        _runnerCancellationSource?.Dispose();
    }
}
