using System;
using System.Collections.Generic;
using System.Reflection;
using NetDaemon.Daemon;
using NetDaemon.Service.App;

namespace NetDaemon.Assemblies;

public interface INetDaemonAssemblyManager
{
    IEnumerable<Assembly> Load(IDaemonAssemblyCompiler compiler, IServiceProvider serviceProvider);

    void ConfigureAssemblies(Action<List<Assembly>, IServiceProvider> configure);
}