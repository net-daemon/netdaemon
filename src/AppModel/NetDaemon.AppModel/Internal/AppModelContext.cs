using NetDaemon.AppModel.Internal.AppFactoryProviders;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext(
    IEnumerable<IAppFactoryProvider> appFactoryProviders,
    IServiceProvider provider,
    FocusFilter focusFilter,
    ILogger<AppModelContext> logger)
    : IAppModelContext
{
    private readonly List<Application> _applications = [];

    private volatile bool _isDisposed;

    public IReadOnlyCollection<IApplication> Applications => _applications;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var factories = appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();

        var filteredFactories = focusFilter.FilterFocusApps(factories);

        foreach (var factory in filteredFactories)
        {
            var app = ActivatorUtilities.CreateInstance<Application>(provider, factory);
            await app.InitializeAsync().ConfigureAwait(false);
            _applications.Add(app);
        }

        logger.LogInformation("Finished loading applications: {State}",
            string.Join(", ", Enum.GetValues<ApplicationState>().Select(possibleState => $"{possibleState} {_applications.Count(app => app.State == possibleState)}")));
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Get all tasks for disposing the apps with a timeout
        var disposeTasks = _applications.Select(app => app.DisposeAsync().AsTask()).ToList();

        await Task.WhenAll(disposeTasks).ConfigureAwait(false);

        _applications.Clear();
    }
}
