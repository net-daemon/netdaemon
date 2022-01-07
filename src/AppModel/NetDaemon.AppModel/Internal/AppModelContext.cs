using System.Reflection;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext : IAppModelContext, IAsyncInitializable
{
    private readonly List<Application> _applications = new();

    private readonly IReadOnlyCollection<Type> _applicationTypes;
    private readonly IEnumerable<IAppTypeResolver> _appTypeResolvers;
    private readonly IAppStateManager? _appStateManager;
    private readonly ILogger<IAppModelContext> _logger;
    private readonly IServiceProvider _provider;
    private bool isDisposed;

    public AppModelContext(
        ILogger<IAppModelContext> logger,
        IEnumerable<IAppTypeResolver> appTypeResolvers,
        IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
        _appTypeResolvers = appTypeResolvers;
        // Can be missing so it is not injected in the constructor
        _appStateManager = provider.GetService<IAppStateManager>();
        _applicationTypes = GetNetDaemonApplicationTypes();
    }

    public IReadOnlyCollection<IApplication> Applications => _applications;
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await LoadApplications();
    }

    public async ValueTask DisposeAsync()
    {
        if (isDisposed)
            return;

        foreach (var appInstance in _applications) await appInstance.DisposeAsync().ConfigureAwait(false);
        _applications.Clear();
        isDisposed = true;
    }

    private async Task LoadApplications()
    {
        foreach (var appType in _applicationTypes)
        {
            var appAttribute = appType.GetCustomAttribute<NetDaemonAppAttribute>();
            var id = appAttribute?.Id ?? appType.FullName ??
                throw new InvalidOperationException("Type was not expected to be null");
            var app = ActivatorUtilities.CreateInstance<Application>(_provider, id, appType);
            await app.InitializeAsync().ConfigureAwait(false);
            _applications.Add(app);
        }
    }

    private List<Type> GetNetDaemonApplicationTypes()
    {
        // Get all classes with the [NetDaemonAppAttribute]
        return _appTypeResolvers.SelectMany(r => r.GetTypes())
            .Where(n => n.IsClass &&
                        !n.IsGenericType &&
                        !n.IsAbstract &&
                        n.GetCustomAttribute<NetDaemonAppAttribute>() != null
            ).ToList();
    }
}