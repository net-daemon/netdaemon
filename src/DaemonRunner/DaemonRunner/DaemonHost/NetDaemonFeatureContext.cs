using System;
using NetDaemon.Assemblies;
using NetDaemon.Daemon;

namespace NetDaemon.DaemonHost;

public class NetDaemonFeatureContext : INetDaemonFeatureContext
{
    internal NetDaemonFeatureContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }
}