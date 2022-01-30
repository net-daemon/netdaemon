using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppAssemblyProviders;

internal class AppAssemblyProvider : IAppAssemblyProvider
{
    private readonly Assembly _assembly;

    public AppAssemblyProvider(Assembly assembly)
    {
        _assembly = assembly;
    }

    public Assembly GetAppAssembly()
    {
        return _assembly;
    }
}