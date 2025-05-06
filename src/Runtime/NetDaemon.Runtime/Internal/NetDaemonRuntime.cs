using System.Reactive.Linq;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace NetDaemon.Runtime.Internal;

internal class NetDaemonRuntime(IHomeAssistantRunner homeAssistantRunner,
        IOptions<HomeAssistantSettings> settings,
        IServiceProvider serviceProvider,
        ILogger<NetDaemonRuntime> logger,
        ICacheManager cacheManager)
    : IRuntime, INetDaemonRuntime
{
    private const string Version = "local build";
    private const int TimeoutInSeconds = 5;

    private readonly TaskCompletionSource _initializationTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly HomeAssistantSettings _haSettings = settings.Value;

    private IAppModelContext? _applicationModelContext;
    private CancellationToken? _stoppingToken;
    private CancellationTokenSource? _runnerCancellationSource;

    public bool IsConnected;

    // These internals are used primarily for testing purposes
    internal IReadOnlyCollection<IApplication> ApplicationInstances =>
        _applicationModelContext?.Applications ?? [];

    private Task _runnerTask = Task.CompletedTask;

    public void Start(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting NetDaemon runtime version {Version}.", Version);

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

            // Assign the runner so we can dispose it later. Note that this task contains the connection loop and will not end. We don't want to await it.
            _runnerTask = homeAssistantRunner.RunAsync(
                _haSettings.Host,
                _haSettings.Port,
                _haSettings.Ssl,
                _haSettings.Token,
                _haSettings.WebsocketPath,
                TimeSpan.FromSeconds(TimeoutInSeconds),
                _runnerCancellationSource.Token);

            // make sure we cancel the task if the stoppingToken is cancelled
            stoppingToken.Register(() => _initializationTcs.TrySetCanceled());
        }
        catch (OperationCanceledException)
        {
            // Ignore and just stop
        }
    }

    public Task WaitForInitializationAsync() => _initializationTcs.Task;

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
                logger.LogWarning("Applications were not successfully disposed during restart, skipping loading apps again");
                return;
            }

            IsConnected = true;

            await cacheManager.InitializeAsync(cancelToken).ConfigureAwait(false);

            await LoadNewAppContextAsync(haConnection, cancelToken);

            // Signal anyone waiting that the runtime is now initialized
            _initializationTcs.TrySetResult();
        }
        catch (Exception ex)
        {
            if (!_initializationTcs.Task.IsCompleted)
            {
                // This means this was the first time we connected and StartAsync is still awaiting _startedAndConnected
                // By setting the exception on the task it will propagate up.
                _initializationTcs.SetException(ex);
            }
            logger.LogCritical(ex, "Error (re-)initializing after connect to Home Assistant");
        }
    }

    private async Task LoadNewAppContextAsync(IHomeAssistantConnection haConnection, CancellationToken cancelToken)
    {
        var appModel = serviceProvider.GetService<IAppModel>();
        if (appModel == null) return;

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
            logger.LogInformation("Home Assistant disconnected due to {Reason}",
                reasonString );
        }

        try
        {
            await DisposeApplicationsAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error disposing applications");
        }

        if (reason == DisconnectReason.Unauthorized)
        {
            logger.LogInformation("Home Assistant runtime will dispose itself to stop automatic retrying to prevent user from being locked out.");
            await DisposeAsync();
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

    private volatile bool _isDisposed;
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
