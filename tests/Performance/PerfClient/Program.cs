using Microsoft.Extensions.Hosting;
using NetDaemon.Runtime;
using NetDaemon.AppModel;
using System.Reflection;
using Serilog;
#pragma warning disable CA1812

try
{
    await Host.CreateDefaultBuilder(args)
              .UseNetDaemonAppSettings()
              .UseSerilog((context, provider, logConfig) =>
              {
                  logConfig.ReadFrom.Configuration(context.Configuration);
              })
              .UseNetDaemonRuntime()
              .ConfigureServices((_, services) =>
                  services
                      .AddAppsFromAssembly(Assembly.GetEntryAssembly()!)
                      // Remove this is you are not running the integration!
                      //.AddNetDaemonStateManager()
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
