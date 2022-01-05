using System.Reflection;

namespace NetDaemon.AppModel.Internal;

internal class AppModelContext : IAppModelContext
{
    private readonly List<Application> _applications = new();

    private readonly IReadOnlyCollection<Type> _applicationTypes;
    private readonly IEnumerable<IAppTypeResolver> _appTypeResolvers;
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
        _applicationTypes = GetNetDaemonApplicationTypes();
        LoadApplications();
    }

    public IReadOnlyCollection<IApplication> Applications => _applications;

    public async ValueTask DisposeAsync()
    {
        if (isDisposed)
            return;

        foreach (var appInstance in _applications) await appInstance.DisposeAsync().ConfigureAwait(false);
        _applications.Clear();
        isDisposed = true;
    }

    private void LoadApplications()
    {
        foreach (var appType in _applicationTypes)
        {
            var appAttribute = appType.GetCustomAttribute<NetDaemonAppAttribute>();
            // The app instance should be created with the Scoped ServiceProvider that is created by the ApplicationContext  
            var id = appAttribute?.Id ?? appType.FullName ??
                throw new InvalidOperationException("Type was not expected to be null");
            try
            {
                _applications.Add(
                    new Application(id,
                        new ApplicationContext(id, appType, _provider)
                    )
                );
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error loading app {id}", id);
            }
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