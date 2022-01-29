using NetDaemon.AppModel.Internal.Resolver;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext : IAppModelContext, IAsyncInitializable
{
    private readonly List<Application> _applications = new();

    private readonly IEnumerable<IAppInstanceResolver> _appInstanceResolvers;
    private readonly IServiceProvider _provider;
    private bool _isDisposed;

    public AppModelContext(IEnumerable<IAppInstanceResolver> appInstanceResolvers, IServiceProvider provider)
    {
        _appInstanceResolvers = appInstanceResolvers;
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
        var appInstances = GetAppInstances().ToList();
        var loadOnlyFocusedApps = ShouldLoadOnlyFocusedApps(appInstances);

        foreach (var appInstance in appInstances)
        {
            if (loadOnlyFocusedApps && !appInstance.HasFocus)
                continue; // We do not load applications that does not have focus attr and we are in focus mode

            var app = ActivatorUtilities.CreateInstance<Application>(_provider, appInstance);
            await app.InitializeAsync().ConfigureAwait(false);
            _applications.Add(app);
        }
    }

    private IEnumerable<IAppInstance> GetAppInstances()
    {
        return _appInstanceResolvers.SelectMany(resolver => resolver.GetApps());
    }

    private static bool ShouldLoadOnlyFocusedApps(IEnumerable<IAppInstance> types)
    {
        return types.Any(instance => instance.HasFocus);
    }
}