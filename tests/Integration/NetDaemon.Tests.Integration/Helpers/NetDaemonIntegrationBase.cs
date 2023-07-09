using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Runtime;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

public class NetDaemonIntegrationBase : IClassFixture<HomeAssistantLifetime>, IAsyncDisposable
{
    public IServiceProvider Services => _scope.ServiceProvider;
    private readonly HomeAssistantLifetime _homeAssistantLifetime;
    private readonly IHost _netDaemon;
    private readonly AsyncServiceScope _scope;

    public NetDaemonIntegrationBase(HomeAssistantLifetime homeAssistantLifetime)
    {
        _homeAssistantLifetime = homeAssistantLifetime;
        _netDaemon = RunWithoutSynchronizationContext(StartNetDaemon);
        _scope = _netDaemon.Services.CreateAsyncScope();
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
                    { "HomeAssistant:Port", _homeAssistantLifetime.Port.ToString() },
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
    
    private T RunWithoutSynchronizationContext<T>(Func<T> func)
    {
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
    }
}