using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Runtime;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

public class NetDaemonIntegrationBase : IClassFixture<HomeAssistantLifetime>
{
    private readonly HomeAssistantLifetime _homeAssistantLifetime;

    public NetDaemonIntegrationBase(HomeAssistantLifetime homeAssistantLifetime)
    {
        _homeAssistantLifetime = homeAssistantLifetime;
    }

    public IHost StartNetDaemon()
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
}