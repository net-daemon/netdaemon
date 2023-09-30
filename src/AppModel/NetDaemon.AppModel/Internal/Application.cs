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

    public bool Enabled { get; private set; }

    public bool IsRunning { get; private set; }

    // TODO: see if we can remove this and refactor the possible states  
    public ApplicationState State =>
                _isErrorState ? ApplicationState.Error :
                IsRunning     ? ApplicationState.Running :
                Enabled       ? ApplicationState.Enabled : 
                                ApplicationState.Disabled;
    
    public string Id => _appFactory.Id;

    public object? Instance => ApplicationContext?.Instance;

    
    public async Task InitializeAsync()
    {
        if (await ShouldInstanceApplicationAsync(Id).ConfigureAwait(false))
        {
            Enabled = true;
            await LoadAsync().ConfigureAwait(false);
        }
    }

    private async Task<bool> ShouldInstanceApplicationAsync(string id)
    {
        if (_appStateManager is null)
            return true;
        return await _appStateManager.GetStateAsync(id).ConfigureAwait(false) == ApplicationState.Enabled;
    }

    public async Task EnableAsync()
    {
        await SaveStateIfStateManagerExistAsync(ApplicationState.Enabled);
        await LoadAsync();
    }
    
    public async Task LoadAsync()
    {
        if (ApplicationContext is not null) return;

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
            IsRunning = true;
            _isErrorState = false;

            _logger.LogInformation("Successfully loaded app {Id}", Id);
        }
        catch (Exception e)
        {
            _isErrorState = true;
            _logger.LogError(e, "Error loading app {Id}", Id);
        }
    }
    
    public async Task DisableAsync()
    {
        await UnloadAsync();
        Enabled = false;
        
        await SaveStateIfStateManagerExistAsync(ApplicationState.Disabled).ConfigureAwait(false);
    }

    public async Task UnloadAsync()
    {
        if (ApplicationContext is not null)
        {
            await ApplicationContext.DisposeAsync().ConfigureAwait(false);
            ApplicationContext = null;
            _logger.LogInformation("Successfully unloaded app {Id}", Id);
        }

        IsRunning = false;
    }    
    
    
    public async ValueTask DisposeAsync()
    {
        await UnloadAsync();
    }

    private async Task SaveStateIfStateManagerExistAsync(ApplicationState appState)
    {
        if (_appStateManager is not null)
            await _appStateManager.SaveStateAsync(Id, appState).ConfigureAwait(false);
    }
}
