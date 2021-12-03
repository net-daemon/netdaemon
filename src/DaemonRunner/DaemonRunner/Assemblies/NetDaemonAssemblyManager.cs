using System;
using System.Collections.Generic;
using System.Reflection;
using NetDaemon.Service.App;

namespace NetDaemon.Assemblies;

public class NetDaemonAssemblyManager : INetDaemonAssemblyManager
{ 
    private bool _loaded { get; set; } = false;
    private List<Action<List<Assembly>>> _configureAssembliesActions = new List<Action<List<Assembly>>>();

    public List<Assembly> LoadedAssemblies { get; set; } = new ();

    public NetDaemonAssemblyManager()
    {
    }
    
    public void Load(IDaemonAssemblyCompiler compiler)
    {
        if (_loaded)
        {
            throw new InvalidOperationException("Assemblies already loaded");
        }
        _loaded = true;
        
        var assemblies = compiler.Load();
        LoadedAssemblies.AddRange(assemblies);
        
        foreach (var configureAssembliesAction in _configureAssembliesActions)
        {
            configureAssembliesAction.Invoke(LoadedAssemblies);
        }
    }

    public void ConfigureAssemblies(Action<List<Assembly>> configure)
    {
        _configureAssembliesActions.Add(configure);
    }
}