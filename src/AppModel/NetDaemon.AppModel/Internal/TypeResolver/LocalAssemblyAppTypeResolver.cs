
using System.Reflection;
using System.Runtime.Loader;

namespace NetDaemon.AppModel.Internal.TypeResolver;

/// <summary>
///     Resolves types from the local assebmly
/// </summary>
internal class LocalAssemblyAppTypeResolver : IAppTypeResolver
{
    public IReadOnlyCollection<Type> GetTypes()
    {
        var binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)!;
        var netDaemonDlls = Directory.GetFiles(binFolder, "NetDaemon.*.dll");

        var alreadyLoadedAssemblies = AssemblyLoadContext.Default.Assemblies
            .Where(x => !x.IsDynamic)
            .Select(x => x.Location)
            .ToList();

        foreach (var netDaemonDllToLoadDynamically in netDaemonDlls.Except(alreadyLoadedAssemblies))
        {
            AssemblyLoadContext.Default.LoadFromAssemblyPath(netDaemonDllToLoadDynamically);
        }

        return AssemblyLoadContext.Default.Assemblies.SelectMany(s => s.GetTypes()).ToList();
    }
}