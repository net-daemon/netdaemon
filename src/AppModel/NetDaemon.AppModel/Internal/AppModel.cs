using System.Linq;
using System.Reflection;

namespace NetDaemon.AppModel.Internal;

internal class AppModelImpl : IAppModel
{
    private readonly ITypeResolver _typeResolver;
    private readonly IServiceProvider _provider;

    public AppModelImpl(
        ITypeResolver typeResolver,
        IServiceProvider provider
    )
    {
        _typeResolver = typeResolver;
        _provider = provider;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public IReadOnlyCollection<IApplicationInstance> LoadApplications(IReadOnlyCollection<string>? skipLoadApplicationList = null)
    {
        // Todo: filter out skipped apps
        var result = new List<IApplicationInstance>();
        foreach (var appType in GetNetDaemonApplicationTypes())
        {
            var instance = ActivatorUtilities.CreateInstance(_provider, appType);
            var id = appType.FullName ?? throw new InvalidOperationException("Type was not expected to be null");
            result.Add(
                new ApplicationContext(id, appType, _provider, instance)
            );
        }
        return result;
    }

    private List<Type> GetNetDaemonApplicationTypes()
    {
        // Get all classes with the [NetDaemonAppAttribute]
        return _typeResolver.GetTypes().Where(n => n.IsClass &&
                                    !n.IsGenericType &&
                                    !n.IsAbstract &&
                                    n.GetCustomAttribute<NetDaemonAppAttribute>() != null
                                    ).ToList();
    }
}