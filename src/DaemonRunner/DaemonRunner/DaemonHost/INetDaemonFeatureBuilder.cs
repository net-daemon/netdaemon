using System;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Assemblies;
using NetDaemon.Daemon;

namespace NetDaemon.DaemonHost;

public interface INetDaemonFeatureBuilder
{
    void AddFeature(Action<INetDaemonFeatureContext, IServiceCollection> feature);
    void Build(IServiceCollection services);
}