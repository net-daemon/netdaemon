using System;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.DaemonHost;

public interface INetDaemonFeatureBuilder
{
    void AddFeature(Action<IServiceCollection> feature);
    void Build(IServiceCollection services);
}