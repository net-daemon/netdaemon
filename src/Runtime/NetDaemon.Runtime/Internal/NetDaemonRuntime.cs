using System.Reactive.Linq;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace NetDaemon.Runtime.Internal;

internal class NetDaemonRuntime : IRuntime
{
    private const int TimeoutInSeconds = 5;
    private readonly IAppModel _appModel;

    private readonly HomeAssistantSettings _haSettings;
    private readonly IHomeAssistantRunner _homeAssistantRunner;

    private readonly ILogger<RuntimeService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IAppModelContext? _applicationModelContext;
    private IHomeAssistantConnection? _connection;

    // These internals are used primarly for testing purposes
    internal IReadOnlyCollection<IApplication>? ApplicationInstances =>
        _applicationModelContext?.Applications ?? Array.Empty<IApplication>();

    public NetDaemonRuntime(
        IHomeAssistantRunner homeAssistantRunner,
        IOptions<HomeAssistantSettings> settings,
        IAppModel appModel,
        IServiceProvider serviceProvider,
        ILogger<RuntimeService> logger)
    {
        _haSettings = settings.Value;
        _homeAssistantRunner = homeAssistantRunner;
        _appModel = appModel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _homeAssistantRunner.OnConnect
            .Select(async c => await OnHomeAssistantClientConnected(c, stoppingToken).ConfigureAwait(false))
            .Subscribe();
        _homeAssistantRunner.OnDisconnect
            .Select(async s => await OnHomeAssistantClientDisconnected(s).ConfigureAwait(false))
            .Subscribe();
        try
        {
            await _homeAssistantRunner.RunAsync(
                _haSettings.Host,
                _haSettings.Port,
                _haSettings.Ssl,
                _haSettings.Token,
                TimeSpan.FromSeconds(TimeoutInSeconds),
                stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore and just stop
        }
    }

    private async Task OnHomeAssistantClientConnected(
        IHomeAssistantConnection haConnection,
        CancellationToken cancelToken
        )
    {
        try
        {
            _logger.LogInformation("Successfully connected to Home Assistant");
            await DependencyInjectionSetup.InitializeAsync2(_serviceProvider, cancelToken).ConfigureAwait(false);

            _applicationModelContext =
                await _appModel.InitializeAsync(CancellationToken.None).ConfigureAwait(false);

            // Handle state change for apps if registered
            var appStateHandler = _serviceProvider.GetService<IHandleHomeAssistantAppStateUpdates>();
            appStateHandler?.Initialize(haConnection, _applicationModelContext);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to intitialize apps");
            throw;
        }
    }

    private async Task OnHomeAssistantClientDisconnected(DisconnectReason reason)
    {
        _logger.LogInformation("HassClient disconnected cause of {reason}, connect retry in {timeout} seconds",
            TimeoutInSeconds, reason);
        if (_connection is not null) _connection = null;
        await DisposeApplicationsAsync().ConfigureAwait(false);
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
}