﻿using System;
using Microsoft.Extensions.Hosting;
using NetDaemon;
using NetDaemon.Model3;

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