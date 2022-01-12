﻿using System;
using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.AppModel;
using System.Reflection;
using NetDaemon.Extensions.Logging;

#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
        .UseNetDaemonAppSettings()
        .UseNetDaemonDefaultLogging()
        .UseNetDaemonRuntime()
        .ConfigureServices((_, services) =>
            services
                .AddAppsFromAssembly(Assembly.GetEntryAssembly()!)
                // Remove this is you are not running the integtration!
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