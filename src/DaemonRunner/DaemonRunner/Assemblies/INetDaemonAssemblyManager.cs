using System;
using System.Collections.Generic;
using System.Reflection;
using NetDaemon.Daemon;
using NetDaemon.Service.App;

namespace NetDaemon.Assemblies;

public interface INetDaemonAssemblyManager
{
    List<Assembly> LoadedAssemblies { get; set; }
    
    void Load(IDaemonAssemblyCompiler compiler);

    void ConfigureAssemblies(Action<List<Assembly>> configure);
}