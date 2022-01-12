using NetDaemon.AppModel.Internal.Extensions;

namespace NetDaemon.AppModel.Internal;

internal class Application : IApplication
{
    private readonly Type _applicationType;
    private readonly IAppStateManager? _appStateManager;
    private readonly bool _hasFocus;
    private readonly bool _loadOnlyFocusedApps;
    private readonly ILogger<Application> _logger;
    private readonly IServiceProvider _provider;

    private bool _isErrorState;

    public Application(
        string id,
        Type applicationType,
        bool loadOnlyFocusedApps,
        ILogger<Application> logger,
        IServiceProvider provider
    )
    {
        Id = id;
        _applicationType = applicationType;
        _loadOnlyFocusedApps = loadOnlyFocusedApps;
        _logger = logger;
        _provider = provider;
        // Can be missing so it is not injected in the constructor
        _appStateManager = provider.GetService<IAppStateManager>();
        _hasFocus = applicationType.HasNetDaemonFocusAttribute();
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
            _logger.LogInformation("Successfully unloaded app {id}", Id);
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
            if (_loadOnlyFocusedApps && !_hasFocus)
                return; // We do not instance apps when we should load only focus apps and not have focus attribute
            ApplicationContext = new ApplicationContext(_applicationType, _provider);

            await SaveStateIfStateManagerExistAsync(ApplicationState.Running);
            _logger.LogInformation("Successfully loaded app {id}", Id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading app {id}", Id);
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