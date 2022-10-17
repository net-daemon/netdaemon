using NetDaemon.AppModel.Internal.AppFactoryProviders;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext : IAppModelContext
{
    private readonly List<Application> _applications = new();

    private readonly IEnumerable<IAppFactoryProvider> _appFactoryProviders;
    private readonly IServiceProvider _provider;
    private readonly FocusFilter _focusFilter;
    private bool _isDisposed;

    public AppModelContext(IEnumerable<IAppFactoryProvider> appFactoryProviders, IServiceProvider provider, FocusFilter focusFilter)
    {
        _appFactoryProviders = appFactoryProviders;
        _provider = provider;
        _focusFilter = focusFilter;
    }

    public IReadOnlyCollection<IApplication> Applications => _applications;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var factories = _appFactoryProviders.SelectMany(provider => provider.GetAppFactories()).ToList();

        var filteredFactories =  _focusFilter.FilterFocusApps(factories);

        foreach (var factory in filteredFactories)
        {
            var app = ActivatorUtilities.CreateInstance<Application>(_provider, factory);
            await app.InitializeAsync().ConfigureAwait(false);
            _applications.Add(app);
        }
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