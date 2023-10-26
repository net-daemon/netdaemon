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

        var filteredFactories = _focusFilter.FilterFocusApps(factories);

        foreach (var factory in filteredFactories)
        {
            var app = ActivatorUtilities.CreateInstance<Application>(_provider, factory);
            await app.InitializeAsync().ConfigureAwait(false);
            _applications.Add(app);
        }

        _logger.LogInformation("Finished loading applications: {State}",
            string.Join(", ", Enum.GetValues<ApplicationState>().Select(possibleState => $"{possibleState} {_applications.Count(app => app.State == possibleState)}")));
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        // Get all tasks for disposing the apps with a timeout
        var disposeTasks = _applications.Select(app => app.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(3))).ToList();

        // We cannot use Task WhenAll here since we do not want to halt on a single exception
        // but rather log all exceptions and make sure all apps are disposed properly
        foreach (var disposeTask in disposeTasks)
        {
            try
            {
               await disposeTask.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var indexOfApp = disposeTasks.IndexOf(disposeTask);
                var app = _applications[indexOfApp];

                if (e is TimeoutException)
                    _logger.LogError("Unloading the app '{App}' takes longer than expected, please check for blocking code", app.Id);
                else
                    _logger.LogError(disposeTask.Exception, "Unloading the app '{App}' failed", app.Id);
            }
        }
        _logger.LogInformation("All apps are disposed");
        _applications.Clear();

        // We need to force a garbage collection here to make sure all apps are freed from memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
