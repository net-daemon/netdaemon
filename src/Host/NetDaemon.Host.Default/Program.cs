using System;
using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.AppModel;

#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
        // .UseDefaultNetDaemonLogging()
        .UseNetDaemonRuntime()
        .ConfigureServices((_, services) => services.AddAppsFromSource()
        )
        .Build()
        .RunAsync()
        .ConfigureAwait(false);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to start host... {e}");
    throw;
}