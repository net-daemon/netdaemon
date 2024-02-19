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
public class NetDaemonIntegrationBase : IAsyncLifetime
{
    public IServiceProvider Services => _scope.ServiceProvider;
    private readonly HomeAssistantLifetime _homeAssistantLifetime;
    private IHost? _netDaemon;
    private AsyncServiceScope _scope;

    public NetDaemonIntegrationBase(HomeAssistantLifetime homeAssistantLifetime)
    {
        _homeAssistantLifetime = homeAssistantLifetime;
    }

    private IHost StartNetDaemon()
    {
        var netDeamon = Host.CreateDefaultBuilder()
            .UseNetDaemonAppSettings()
            .UseNetDaemonRuntime()
            .ConfigureAppConfiguration((context, config) =>
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

        netDeamon.Start();
        return netDeamon;
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

    public async Task InitializeAsync()
    {
        // Some test frameworks like xUnit use a custom synchronization context that causes deadlocks when blocking on async code (such as IHost.Start) especially on machines with less resources.
        _netDaemon = RunWithoutSynchronizationContext(StartNetDaemon);
        _scope = _netDaemon.Services.CreateAsyncScope();
        // Wait for the NetDaemon to connect to Home Assistant
        // It is never a good idea to use delays to make sure NetDaemon is connected correctly
        // but I have no good ideas how to solve this in a better way right now.
        // It fails sometimes due to it have not filled the state cache before the tests starts
        await Task.Delay(5000);
    }

    public async Task DisposeAsync()
    {

            await _scope.DisposeAsync();

        if (_netDaemon is not null)
        {
            _netDaemon.Dispose();
        }
    }
}
