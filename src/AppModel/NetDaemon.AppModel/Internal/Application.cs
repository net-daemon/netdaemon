using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Internal;

internal class Application : IApplication
{
    private const int MaxTimeInInitializeAsyncInMs = 5000;

    private readonly IServiceProvider _provider;
    private readonly ILogger<Application> _logger;
    private readonly IAppFactory _appFactory;
    private readonly IAppStateManager? _appStateManager;

    private bool _isErrorState;

    public Application(IServiceProvider provider, ILogger<Application> logger, IAppFactory appFactory)
    {
        _provider = provider;
        _logger = logger;
        _appFactory = appFactory;

        // Can be missing so it is not injected in the constructor
        _appStateManager = provider.GetService<IAppStateManager>();
    }

    // Used in tests
    internal ApplicationContext? ApplicationContext { get; private set; }

    public string Id => _appFactory.Id;

    public ApplicationState State
    {
        get
        {
            if (_isErrorState)
                return ApplicationState.Error;

            return ApplicationContext is null ? ApplicationState.Disabled : ApplicationState.Running;
        }
    }

    public async Task SetStateAsync(ApplicationState state)
    {
        switch (state)
        {
            case ApplicationState.Enabled:
                await LoadApplication(state);
                break;
            case ApplicationState.Disabled:
                await UnloadApplication(state);
                break;
            case ApplicationState.Error:
                _isErrorState = true;
                await SaveStateIfStateManagerExistAsync(state);
                break;
            case ApplicationState.Running:
                throw new ArgumentException("Running state can only be set internally", nameof(state));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (ApplicationContext is not null) await ApplicationContext.DisposeAsync().ConfigureAwait(false);
    }

    private async Task UnloadApplication(ApplicationState state)
    {
        if (ApplicationContext is not null)
        {
            await ApplicationContext.DisposeAsync().ConfigureAwait(false);
            ApplicationContext = null;
            _logger.LogInformation("Successfully unloaded app {Id}", Id);
            await SaveStateIfStateManagerExistAsync(state).ConfigureAwait(false);
        }
    }

    private async Task LoadApplication(ApplicationState state)
    {
        // first we save state "Enabled", this will also
        // end up being state "Running" if instancing is successful
        // or "Error" if instancing the app fails
        await SaveStateIfStateManagerExistAsync(state);
        if (ApplicationContext is null)
            await InstanceApplication().ConfigureAwait(false);
    }

    public async Task InitializeAsync()
    {
        if (await ShouldInstanceApplicationAsync(Id).ConfigureAwait(false))
            await InstanceApplication().ConfigureAwait(false);
    }

    private async Task InstanceApplication()
    {
        try
        {
            ApplicationContext = new ApplicationContext(_provider, _appFactory);

            // Init async and warn user if taking too long.
            var initAsyncTask = ApplicationContext.InitializeAsync();
            var timeoutTask = Task.Delay(MaxTimeInInitializeAsyncInMs);
            await Task.WhenAny(initAsyncTask, timeoutTask).ConfigureAwait(false);
            if (timeoutTask.IsCompleted)
                _logger.LogWarning(
                    "InitializeAsync is taking too long to execute in application {Id}, this function should not be blocking",
                    Id);

            await initAsyncTask; // Continue to wait even if timeout is set so we do not miss errors

            await SaveStateIfStateManagerExistAsync(ApplicationState.Running);
            _logger.LogInformation("Successfully loaded app {Id}", Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading app {Id}", Id);
            await SetStateAsync(ApplicationState.Error);
        }
    }

    private async Task SaveStateIfStateManagerExistAsync(ApplicationState appState)
    {
        if (_appStateManager is not null)
            await _appStateManager.SaveStateAsync(Id, appState).ConfigureAwait(false);
    }

    private async Task<bool> ShouldInstanceApplicationAsync(string id)
    {
        if (_appStateManager is null)
            return true;
        return await _appStateManager.GetStateAsync(id).ConfigureAwait(false)
               == ApplicationState.Enabled;
    }
}
