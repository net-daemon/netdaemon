using System.Reflection;
using System.Runtime.Loader;

namespace NetDaemon.AppModel.Internal.Compiler;

internal class CollectableAssemblyLoadContext : AssemblyLoadContext
{
    public CollectableAssemblyLoadContext() : base(true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return null;
    }
}