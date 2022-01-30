using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppAssemblyProviders;

internal interface IAppAssemblyProvider
{
    public Assembly GetAppAssembly();
}