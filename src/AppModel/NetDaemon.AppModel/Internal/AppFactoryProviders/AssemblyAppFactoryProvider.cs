using System.Reflection;
using NetDaemon.AppModel.Internal.AppAssemblyProviders;
using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Internal.AppFactoryProviders;

internal class AssemblyAppFactoryProvider : IAppFactoryProvider
{
    private readonly IEnumerable<IAppAssemblyProvider> _assemblyResolvers;

    public AssemblyAppFactoryProvider(IEnumerable<IAppAssemblyProvider> assemblyResolvers)
    {
        _assemblyResolvers = assemblyResolvers;
    }

    public IReadOnlyCollection<IAppFactory> GetAppFactories()
    {
        return _assemblyResolvers
            .Select(resolver => resolver.GetAppAssembly())
            .SelectMany(assembly => assembly.GetTypes())
            .Where(IsNetDaemonAppType)
            .Select(type => FuncAppFactory.Create(type))
            .ToList();
    }

    private static bool IsNetDaemonAppType(Type type)
    {
        if (!type.IsClass || type.IsGenericType || type.IsAbstract)
        {
            return false;
        }

        return type.GetCustomAttribute<NetDaemonAppAttribute>() is not null;
    }
}