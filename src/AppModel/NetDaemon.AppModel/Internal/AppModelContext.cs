using System.Reflection;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext : IAppModelContext, IAsyncInitializable
{
    private readonly List<Application> _applications = new();

    private readonly IEnumerable<IAppTypeResolver> _appTypeResolvers;
    private readonly IServiceProvider _provider;
    private bool _isDisposed;

    public AppModelContext(
        IEnumerable<IAppTypeResolver> appTypeResolvers,
        IServiceProvider provider)
    {
        _provider = provider;
        _appTypeResolvers = appTypeResolvers;
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
        var applicationTypes = GetNetDaemonApplicationTypes().ToArray();
        var loadOnlyFocusedApps = ShouldLoadOnlyFocusedApps(applicationTypes);
        foreach (var appType in applicationTypes)
        {
            if (loadOnlyFocusedApps && !HasFocusAttribute(appType))
                continue; // We do not load applications that does not have focus attr and we are in focus mode

            var appAttribute = appType.GetCustomAttribute<NetDaemonAppAttribute>();
            var id = appAttribute?.Id ?? appType.FullName ??
                throw new InvalidOperationException("Type was not expected to be null");

            var app = ActivatorUtilities.CreateInstance<Application>(_provider, id, appType);
            await app.InitializeAsync().ConfigureAwait(false);
            _applications.Add(app);
        }
    }

    private IEnumerable<Type> GetNetDaemonApplicationTypes()
    {
        // Get all classes with the [NetDaemonAppAttribute]
        return _appTypeResolvers.SelectMany(r => r.GetTypes())
            .Where(n => n.IsClass &&
                        !n.IsGenericType &&
                        !n.IsAbstract &&
                        n.GetCustomAttribute<NetDaemonAppAttribute>() != null
            );
    }

    private static bool ShouldLoadOnlyFocusedApps(IEnumerable<Type> types)
    {
        return types.Any(n => n.GetCustomAttribute<FocusAttribute>() is not null);
    }

    private static bool HasFocusAttribute(Type type)
    {
        return type.GetCustomAttribute<FocusAttribute>() is not null;
    }
}