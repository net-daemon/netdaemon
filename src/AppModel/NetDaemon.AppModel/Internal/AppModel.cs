using System.Reflection;

namespace NetDaemon.AppModel.Internal;

internal class AppModelImpl : IAppModel
{
    private readonly IEnumerable<IAppTypeResolver> _appTypeResolvers;
    private readonly IServiceProvider _provider;

    public AppModelImpl(
        IEnumerable<IAppTypeResolver> appTypeResolvers,
        IServiceProvider provider
    )
    {
        _appTypeResolvers = appTypeResolvers;
        _provider = provider;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    // Maybe the AppModelImpl should keep the list of created apps internal instead of returning it, so this class is also responsible
    // for disposing them

    public IReadOnlyCollection<IApplicationInstance> LoadApplications(IReadOnlyCollection<string>? skipLoadApplicationList = null)
    {
        var result = new List<IApplicationInstance>();
        foreach (var appType in GetNetDaemonApplicationTypes())
        {
            // The app instance should be created with the Scoped ServcieProvider that is created by the  ApplicationContext  
            var id = appType.FullName ?? throw new InvalidOperationException("Type was not expected to be null");
            // Check if the app should be skipped before adding it
            if (!skipLoadApplicationList?.Contains(appType.FullName) ?? true)
            {
                result.Add(
                    new ApplicationContext(id, appType, _provider)
                );
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