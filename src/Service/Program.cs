using System;
using Microsoft.Extensions.Hosting;
using NetDaemon;
using NetDaemon.HassModel;

try
{
    await Host.CreateDefaultBuilder(args)
        .UseDefaultNetDaemonLogging()
        .UseNetDaemon()
        .UseAppScopedHaContext()
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