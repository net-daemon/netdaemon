using System.Reflection;

namespace NetDaemon.AppModel.Internal;

internal interface IAssemblyResolver
{
    public Assembly GetResolvedAssembly();
}
