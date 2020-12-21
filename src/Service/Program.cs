using System;
using Microsoft.Extensions.Hosting;
using NetDaemon;

try
{
    await Host.CreateDefaultBuilder(args)
        .UseDefaultNetDaemonLogging()
        .UseNetDaemon()
        .Build()
        .RunAsync();
}
catch (Exception e)
{
    Console.WriteLine($"Failed to start host... {e}");
}
finally
{
    NetDaemonExtensions.CleanupNetDaemon();
}

