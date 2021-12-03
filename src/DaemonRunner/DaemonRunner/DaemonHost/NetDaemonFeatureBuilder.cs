using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.DaemonHost;

public class NetDaemonFeatureBuilder : INetDaemonFeatureBuilder
{
    private bool _hostBuilt;

    public IList<Action<IServiceCollection>> Features { get; } =
        new List<Action<IServiceCollection>>();
    
    public void AddFeature(Action<IServiceCollection> feature)
    {
        Features.Add(feature);
    }

    public void Build(IServiceCollection services)
    {
        if (_hostBuilt)
        {
            throw new InvalidOperationException("App already built");
        }
        _hostBuilt = true;
        
        foreach (var feature in Features)
        {
            feature.Invoke(services);
        }
    }
}