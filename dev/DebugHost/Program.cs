using System;
using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.AppModel;
using System.Reflection;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Tts;

#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
        .UseNetDaemonAppSettings()
        .UseNetDaemonDefaultLogging()
        .UseNetDaemonRuntime()
        .UseNetDaemonTextToSpeech()
        .ConfigureServices((_, services) =>
            services
                // change type of compilation here
                // .AddAppsFromSource(true)
                .AddAppsFromAssembly(Assembly.GetEntryAssembly()!)
                // Remove this is you are not running the integration!
                .AddNetDaemonStateManager()
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
