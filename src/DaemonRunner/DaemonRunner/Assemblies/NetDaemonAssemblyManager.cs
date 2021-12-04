using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NetDaemon.Service.App;

namespace NetDaemon.Assemblies;

public class NetDaemonAssemblyManager : INetDaemonAssemblyManager
{ 
    private bool _loaded { get; set; }
    private List<Action<List<Assembly>, IServiceProvider>> _configureAssembliesActions = new List<Action<List<Assembly>, IServiceProvider>>();

    public IEnumerable<Assembly> Load(IDaemonAssemblyCompiler compiler, IServiceProvider serviceProvider)
    {
        if (_loaded)
        {
            throw new InvalidOperationException("Assemblies already loaded");
        }
        _loaded = true;

        var assemblies = compiler.Load().ToList();
        
        foreach (var configureAssembliesAction in _configureAssembliesActions)
        {
            configureAssembliesAction.Invoke(assemblies, serviceProvider);
        }

        return assemblies;
    }

    public void ConfigureAssemblies(Action<List<Assembly>, IServiceProvider> configure)
    {
        _configureAssembliesActions.Add(configure);
    }
}