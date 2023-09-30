using NetDaemon.AppModel.Internal.AppFactoryProviders;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext : IAppModelContext
{
    private readonly List<Application> _applications;

    private readonly ILogger<AppModelContext> _logger;
    private bool _isDisposed;

    public AppModelContext(IEnumerable<IAppFactoryProvider> appFactoryProviders, IServiceProvider provider, FocusFilter focusFilter, ILogger<AppModelContext> logger)
    {
        _logger = logger;
        
        var factories = appFactoryProviders.SelectMany(p => p.GetAppFactories()).ToList();

        var filteredFactories =  focusFilter.FilterFocusApps(factories);

        _applications = filteredFactories.Select(factory => ActivatorUtilities.CreateInstance<Application>(provider, factory))
            .ToList();
    }

    public IReadOnlyCollection<IApplication> Applications => _applications.AsReadOnly();

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
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