using System;
using System.Collections.Generic;
using System.Reflection;
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
#pragma warning disable CS0649
    private IHost _netDaemon = null!;
#pragma warning restore CS0649
#pragma warning disable CS0649
    private AsyncServiceScope _scope;
#pragma warning restore CS0649

    public NetDaemonIntegrationBase(HomeAssistantLifetime homeAssistantLifetime)
    {
        _homeAssistantLifetime = homeAssistantLifetime;
        //_netDaemon = StartNetDaemon();
        //_scope = _netDaemon.Services.CreateAsyncScope();
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

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        _netDaemon.Dispose();
    }
}