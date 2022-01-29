using System.Reflection;

namespace NetDaemon.AppModel.Internal.TypeResolver;

internal class AppTypeResolver : IAppTypeResolver
{
    private readonly IEnumerable<IAssemblyResolver> _assemblyResolvers;

    public AppTypeResolver(IEnumerable<IAssemblyResolver> assemblyResolvers)
    {
        _assemblyResolvers = assemblyResolvers;
    }

    public IReadOnlyCollection<Type> GetTypes()
    {
        return _assemblyResolvers
            .Select(resolver => resolver.GetResolvedAssembly())
            .SelectMany(assembly => assembly.GetTypes())
            .Where(IsNetDaemonAppType)
            .ToList();
    }

    private static bool IsNetDaemonAppType(Type type)
    {
        if (!type.IsClass || !type.IsGenericType || !type.IsAbstract)
        {
            return false;
        }

        return type.GetCustomAttribute<NetDaemonAppAttribute>() is not null;
    }
}