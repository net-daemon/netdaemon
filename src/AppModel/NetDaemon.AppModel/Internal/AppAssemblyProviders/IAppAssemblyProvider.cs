using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppAssemblyProviders;

public interface IAppAssemblyProvider
{
    public Assembly GetAppAssembly();
}
