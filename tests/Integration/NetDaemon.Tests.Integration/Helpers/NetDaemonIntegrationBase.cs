using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Runtime;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

[Collection("HomeAssistant collection")]
public class NetDaemonIntegrationBase : IAsyncDisposable
{
    public IServiceProvider Services => _scope.ServiceProvider;
    private readonly HomeAssistantLifetime _homeAssistantLifetime;
    private readonly IHost _netDaemon;
    private readonly AsyncServiceScope _scope;

    public NetDaemonIntegrationBase(HomeAssistantLifetime homeAssistantLifetime)
    {
        _homeAssistantLifetime = homeAssistantLifetime;
        // Some test frameworks like xUnit use a custom synchronization context that causes deadlocks when blocking on async code (such as IHost.Start) especially on machines with less resources.
        _netDaemon = RunWithoutSynchronizationContext(StartNetDaemon);
        _scope = _netDaemon.Services.CreateAsyncScope();
    }

    private IHost StartNetDaemon()
    {
        var netDaemon = Host.CreateDefaultBuilder()
            .UseNetDaemonAppSettings()
            .UseNetDaemonRuntime()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "HomeAssistant:Port", _homeAssistantLifetime.Port.ToString(CultureInfo.InvariantCulture) },
                    { "HomeAssistant:Token", _homeAssistantLifetime.AccessToken }
                });
            })
            .ConfigureServices((_, services) =>
                services
                    .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
                    .AddNetDaemonStateManager()
            ).Build();

        netDaemon.Start();

        WaitForRuntimeToBeInitialized(netDaemon).GetAwaiter().GetResult();

        return netDaemon;
    }

    private static async Task WaitForRuntimeToBeInitialized(IHost host)
    {
        var netDaemonRuntime = host.Services.GetRequiredService<INetDaemonRuntime>();
        await netDaemonRuntime.WaitForInitializationAsync();
    }

    /// <summary>
    /// Runs the specified function without a synchronization context and restores the synchronization context afterwards.
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static T RunWithoutSynchronizationContext<T>(Func<T> func)
    {
        // Capture the current synchronization context so we can restore it later.
        // We don't have to be afraid of other threads here as this is a ThreadStatic.
        var synchronizationContext = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            return func();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        _netDaemon.Dispose();
        GC.SuppressFinalize(this);
    }
}
