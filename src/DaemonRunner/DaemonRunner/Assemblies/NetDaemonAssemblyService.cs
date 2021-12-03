using System;
using System.Collections.Generic;
using System.Reflection;
using NetDaemon.Daemon;
using NetDaemon.Service.App;

namespace NetDaemon.Assemblies;

public class NetDaemonAssemblyService : INetDaemonAssemblies
{
    internal NetDaemonAssemblyService(List<Assembly> loadedAssemblies)
    {
        LoadedAssemblies = loadedAssemblies.AsReadOnly();
    }

    public IReadOnlyList<Assembly> LoadedAssemblies { get; }
}