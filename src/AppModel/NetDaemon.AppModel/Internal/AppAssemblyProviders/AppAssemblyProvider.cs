using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppAssemblyProviders;

internal class AppAssemblyProvider(Assembly assembly) : IAppAssemblyProvider
{
    public Assembly GetAppAssembly()
    {
        return assembly;
    }
}
