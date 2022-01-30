using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppAssemblyProvider;

internal interface IAppAssemblyProvider
{
    public Assembly GetAppAssembly();
}