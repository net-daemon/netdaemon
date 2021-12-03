using System.Collections.Generic;
using System.Reflection;

namespace NetDaemon.Daemon;

public interface INetDaemonAssemblies
{
    IReadOnlyList<Assembly> LoadedAssemblies { get; }
}