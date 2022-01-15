using System;
using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime.Internal.Extensions;

#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
        // .UseDefaultNetDaemonLogging()
        .UseNetDaemonAddon()
        .Build()
        .RunAsync()
        .ConfigureAwait(false);
}
catch (Exception e)
{
    Console.WriteLine($"Failed to start host... {e}");
    throw;
}