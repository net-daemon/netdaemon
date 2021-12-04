using System;

namespace NetDaemon.Assemblies;

public interface INetDaemonFeatureContext
{
    IServiceProvider ServiceProvider { get; }
}