using System.Reflection;
using System.Runtime.Loader;

namespace NetDaemon.AppModel.Internal.Compiler;

public class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    public CollectibleAssemblyLoadContext() : base(true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        return null;
    }
}
