using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Assemblies;
using NetDaemon.Daemon;

namespace NetDaemon.DaemonHost;

public class NetDaemonFeatureBuilder : INetDaemonFeatureBuilder
{
    private bool _hostBuilt;

    public IList<Action<INetDaemonFeatureContext, IServiceCollection>> Features { get; } =
        new List<Action<INetDaemonFeatureContext, IServiceCollection>>();

    public void AddFeature(Action<INetDaemonFeatureContext, IServiceCollection> feature)
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

        INetDaemonFeatureContext context = GenerateFeaturesContext(services);

        foreach (var feature in Features)
        {
            feature.Invoke(context, services);
        }
    }

    private static INetDaemonFeatureContext GenerateFeaturesContext(IServiceCollection services)
    {
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        INetDaemonFeatureContext context = new NetDaemonFeatureContext(serviceProvider);
        services.AddSingleton<INetDaemonFeatureContext>(context);
        return context;
    }
}