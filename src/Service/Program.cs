using System;
using Microsoft.Extensions.Hosting;
using NetDaemon;
using NetDaemon.DaemonHost;

#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
        .UseNetDaemonBuilder()
        .UseDefaultNetDaemonLogging()
        .UseNetDaemon()
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