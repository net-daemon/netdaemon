using NetDaemon.AppModel.Internal.AppFactoryProviders;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext : IAppModelContext
{
    private readonly List<Application> _applications = new();

    private readonly IEnumerable<IAppFactoryProvider> _appFactoryProviders;
    private readonly IServiceProvider _provider;
    private readonly FocusFilter _focusFilter;
    private ILogger<AppModelContext> _logger;
    private bool _isDisposed;

    public AppModelContext(IEnumerable<IAppFactoryProvider> appFactoryProviders, IServiceProvider provider, FocusFilter focusFilter, ILogger<AppModelContext> logger)
    {
        _appFactoryProviders = appFactoryProviders;
        _provider = provider;
        _focusFilter = focusFilter;
        _logger = logger;
    }

    public IReadOnlyCollection<IApplication> Applications => _applications;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var factories = _appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();

        var filteredFactories =  _focusFilter.FilterFocusApps(factories);

        foreach (var factory in filteredFactories)
        {
            var app = ActivatorUtilities.CreateInstance<Application>(_provider, factory);
            _applications.Add(app);
        }
        
        foreach (var application in _applications)
        {
            await application.InitializeAsync();
        }

        _logger.LogInformation("Finished loading applications: {state}",
            string.Join(", ", Enum.GetValues<ApplicationState>().Select(possibleState => $"{possibleState} {_applications.Count(app => app.State == possibleState)}")));
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        foreach (var appInstance in _applications)
        {
            await appInstance.DisposeAsync().ConfigureAwait(false);
        }
        
        _applications.Clear();
    }
}