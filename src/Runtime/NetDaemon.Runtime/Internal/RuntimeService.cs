using System.Reactive.Linq;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

namespace NetDaemon.Runtime.Internal;

internal class RuntimeService : BackgroundService
{
    private const int TimeoutInSeconds = 5;
    private readonly IAppModel _appModel;

    private readonly HomeAssistantSettings _haSettings;
    private readonly IHomeAssistantRunner _homeAssistantRunner;

    private readonly IHostApplicationLifetime _hostLifetime;

    private readonly ILogger<RuntimeService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IReadOnlyCollection<IApplicationInstance>? _applicationInstances;
    private IHomeAssistantConnection? _connection;
    private bool _hassModelIsInitialized;

    public RuntimeService(
        IHostApplicationLifetime hostLifetime,
        IHomeAssistantRunner homeAssistantRunner,
        IOptions<HomeAssistantSettings> settings,
        IAppModel appModel,
        IServiceProvider serviceProvider,
        ILogger<RuntimeService> logger)
    {
        _haSettings = settings.Value;
        _hostLifetime = hostLifetime;
        _homeAssistantRunner = homeAssistantRunner;
        _appModel = appModel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _homeAssistantRunner.OnConnect
            .Select(async s => await OnHomeAssistantClientConnected(s, stoppingToken).ConfigureAwait(false))
            .Subscribe();
        _homeAssistantRunner.OnDisconnect
            .Select(async s => await OnHomeAssistantClientDisconnected(s).ConfigureAwait(false))
            .Subscribe();
        await _homeAssistantRunner.RunAsync(
            _haSettings.Host,
            _haSettings.Port,
            _haSettings.Ssl,
            _haSettings.Token,
            TimeSpan.FromSeconds(TimeoutInSeconds),
            stoppingToken).ConfigureAwait(false);

        // Stop application if this is exited
        _hostLifetime.StopApplication();
    }

    private async Task OnHomeAssistantClientConnected(IHomeAssistantConnection connection,
        CancellationToken cancelToken)
    {
        try
        {
            _logger.LogInformation("Successfully connected to Home Assistant");
            if (!_hassModelIsInitialized)
                await DependencyInjectionSetup.InitializeAsync2(_serviceProvider, cancelToken).ConfigureAwait(false);
            _hassModelIsInitialized = true;
            _applicationInstances = _appModel.LoadApplications();
            foreach (var appInstance in _applicationInstances)
            {
                _logger.LogInformation("Successfully loaded app {id}", appInstance.Id);
            }
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
        if (_applicationInstances is not null)
        {
            foreach (var applicationInstance in _applicationInstances)
                await applicationInstance.DisposeAsync().ConfigureAwait(false);
            _applicationInstances = null;
        }
    }
}