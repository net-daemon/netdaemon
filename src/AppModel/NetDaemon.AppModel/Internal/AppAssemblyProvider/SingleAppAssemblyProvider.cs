using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppAssemblyProvider;

internal class SingleAppAssemblyProvider : IAppAssemblyProvider
{
    private readonly Assembly _assembly;

    public SingleAppAssemblyProvider(Assembly assembly)
    {
        _assembly = assembly;
    }

    public Assembly GetAppAssembly()
    {
        return _assembly;
    }
}