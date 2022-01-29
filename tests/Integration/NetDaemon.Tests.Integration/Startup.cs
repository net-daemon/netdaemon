using System.Reflection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Runtime;

namespace NetDaemon.Tests.Integration;

/// <summary>
///     Startup class
/// </summary>
/// <remarks>
///     XUnit.DependencyInjection logic finds the startup here and we can make our custom
///     host builder that allows dependency injection in test classes
/// </remarks>
public class Startup
{
    public IHostBuilder CreateHostBuilder(AssemblyName assemblyName)
    {
        return Host.CreateDefaultBuilder()
            .UseNetDaemonAppSettings()
            .UseNetDaemonRuntime()
            .ConfigureServices((_, services) =>
                services
                    .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
                    .AddNetDaemonStateManager()
            );
    }
}
