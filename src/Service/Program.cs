using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Model3;
using NetDaemon;
using NetDaemon.Common.ModelV3;

try
{
    await Host.CreateDefaultBuilder(args)
        .UseDefaultNetDaemonLogging()
        .UseNetDaemon()
        .UseNetDaemonSingletonServices()
        .UseHaContext()
        .Build()
        .RunAsync()
        .ConfigureAwait(false);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to start host... {e}");
    throw;
}
finally
{
    NetDaemonExtensions.CleanupNetDaemon();
}