using System.Reflection;

namespace NetDaemon.AppModel.Internal.AppAssemblyProviders;

/// <summary>
///     Provides interface for applications residing in assemblies
/// </summary>
public interface IAppAssemblyProvider
{
    /// <summary>
    ///     Gets the assembly that has the NetDaemon applications
    /// </summary>
    public Assembly GetAppAssembly();
}
