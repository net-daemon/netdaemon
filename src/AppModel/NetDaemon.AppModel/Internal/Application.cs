namespace NetDaemon.AppModel.Internal;

internal class Application : IApplication
{
    private readonly IAppStateManager? _appStateManager;
    private readonly IServiceProvider _provider;
    private readonly Type _applicationType;
    private readonly ILogger<IApplication> _logger;

    private bool _isErrorState;

    public Application(
        string id,
        Type applicationType,
        ILogger<IApplication> logger,
        IServiceProvider provider
    )
    {
        Id = id;
        _applicationType = applicationType;
        _logger = logger;
        _provider = provider;

        // Can be missing so it is not injected in the constructor
        _appStateManager = provider.GetService<IAppStateManager>();
    }

    public async Task InitializeAsync()
    {
        if (await ShouldInstanceApplicationAsync(Id).ConfigureAwait(false))
        {
            await InstanceApplication().ConfigureAwait(false);
        }
    }

    private async Task InstanceApplication()
    {
        try
        {
            ApplicationContext = new ApplicationContext(Id, _applicationType, _provider);
            await SaveStateIfStateManagerExistAsync(ApplicationState.Running);
            _logger.LogInformation("Successfully loaded app {id}", Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading app {id}", Id);
            await SetStateAsync(ApplicationState.Error);
        }
    }

    // Used in tests
    internal ApplicationContext? ApplicationContext { get; set; }

    public string Id { get; }

    public ApplicationState State
    {
        get
        {
            if (_isErrorState)
                return ApplicationState.Error;

            return ApplicationContext is null ? ApplicationState.Disabled :
                    ApplicationState.Running;
        }
    }

    public async Task SetStateAsync(ApplicationState state)
    {
        switch (state)
        {
            case ApplicationState.Enabled:
                // first we save state "Enabled", this will also
                // endup being state "Running" if instancing is successful
                // or "Error" if instancing the app fails
                await SaveStateIfStateManagerExistAsync(state);
                if (ApplicationContext is null)
                    await InstanceApplication().ConfigureAwait(false);
                break;
            case ApplicationState.Disabled:
                if (ApplicationContext is not null)
                {
                    await ApplicationContext.DisposeAsync().ConfigureAwait(false);
                    ApplicationContext = null;
                    _logger.LogInformation("Successfully unloaded app {id}", Id);
                    await SaveStateIfStateManagerExistAsync(state).ConfigureAwait(false);
                }
                break;
            case ApplicationState.Error:
                _isErrorState = true;
                await SaveStateIfStateManagerExistAsync(state);
                break;
            case ApplicationState.Running:
                throw new ArgumentException("Running state can only be set internally", nameof(state));
        }
    }

    private async Task SaveStateIfStateManagerExistAsync(ApplicationState appState)
    {
        if (_appStateManager is not null)
            await _appStateManager.SaveStateAsync(Id, appState).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (ApplicationContext is not null) await ApplicationContext.DisposeAsync().ConfigureAwait(false);
    }

    private async Task<bool> ShouldInstanceApplicationAsync(string id)
    {
        if (_appStateManager is null)
            return true;
        return await _appStateManager.GetStateAsync(id).ConfigureAwait(false)
            == ApplicationState.Enabled;
    }
}