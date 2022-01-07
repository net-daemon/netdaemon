using System;
using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.AppModel;

#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
        .UseNetDaemonRuntime()
        .ConfigureServices((_, services) =>
            services
                .AddAppsFromSource()
                .AddNetDameonStateManager()
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