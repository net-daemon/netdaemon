using NetDaemon.AppModel.Internal.AppFactoryProvider;
using IAppFactory = NetDaemon.AppModel.Internal.AppFactory.IAppFactory;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext : IAppModelContext, IAsyncInitializable
{
    private readonly List<Application> _applications = new();

    private readonly IEnumerable<IAppFactoryProvider> _appFactoryProviders;
    private readonly IServiceProvider _provider;
    private bool _isDisposed;

    public AppModelContext(IEnumerable<IAppFactoryProvider> appFactoryProviders, IServiceProvider provider)
    {
        _appFactoryProviders = appFactoryProviders;
        _provider = provider;
    }

    public IReadOnlyCollection<IApplication> Applications => _applications;

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        foreach (var appInstance in _applications) await appInstance.DisposeAsync().ConfigureAwait(false);
        _applications.Clear();
        _isDisposed = true;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await LoadApplications();
    }

    private async Task LoadApplications()
    {
        var factories = GetAppFactories().ToList();
        var loadOnlyFocusedApps = ShouldLoadOnlyFocusedApps(factories);

        foreach (var factory in factories)
        {
            if (loadOnlyFocusedApps && !factory.HasFocus)
                continue; // We do not load applications that does not have focus attr and we are in focus mode

            var app = ActivatorUtilities.CreateInstance<Application>(_provider, factory);
            await app.InitializeAsync().ConfigureAwait(false);
            _applications.Add(app);
        }
    }

    private IEnumerable<IAppFactory> GetAppFactories()
    {
        return _appFactoryProviders.SelectMany(provider => provider.GetAppFactories());
    }

    private static bool ShouldLoadOnlyFocusedApps(IEnumerable<IAppFactory> factories)
    {
        return factories.Any(factory => factory.HasFocus);
    }
}