using System.Reflection;

namespace NetDaemon.AppModel.Internal;

internal class AppModelImpl : IAppModel
{
    private readonly IEnumerable<IAppTypeResolver> _appTypeResolvers;
    private readonly ILogger<IAppModel> _logger;
    private readonly IServiceProvider _provider;

    public AppModelImpl(
        IEnumerable<IAppTypeResolver> appTypeResolvers,
        ILogger<IAppModel> logger,
        IServiceProvider provider
    )
    {
        _appTypeResolvers = appTypeResolvers;
        _logger = logger;
        _provider = provider;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public IReadOnlyCollection<IApplicationInstance> LoadApplications(
        IReadOnlyCollection<string>? skipLoadApplicationList = null)
    {
        var result = new List<IApplicationInstance>();
        foreach (var appType in GetNetDaemonApplicationTypes())
        {
            var appAttribute = appType.GetCustomAttribute<NetDaemonAppAttribute>();
            // The app instance should be created with the Scoped ServiceProvider that is created by the ApplicationContext  
            var id = appAttribute?.Id ?? appType.FullName ??
                throw new InvalidOperationException("Type was not expected to be null");
            // Check if the app should be skipped before adding it
            if (!skipLoadApplicationList?.Contains(appType.FullName) ?? true)
            {
                try
                {
                    result.Add(
                        new ApplicationContext(id, appType, _provider)
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error loading app {id}", id);
                }
            }
        }

        return result;
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