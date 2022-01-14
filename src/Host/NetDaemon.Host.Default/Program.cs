using System;
using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.AppModel;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Scheduler;

#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
        .UseNetDaemonAppSettings()
        .UseNetDaemonDefaultLogging()
        .UseNetDaemonRuntime()
        .ConfigureServices((_, services) =>
            services
                .AddAppsFromSource()
                .AddNetDameonStateManager()
                .AddNetDaemonScheduler()
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