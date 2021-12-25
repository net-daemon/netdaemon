using System.Reflection;
using System.Runtime.Loader;

namespace NetDaemon.AppModel.Internal.Compiler;

public class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    public CollectibleAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    protected override Assembly? Load(AssemblyName assemblyName) => null;
}
