using System;
using System.IO;
using System.Threading.Tasks;
using NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

public class HomeAssistantLifetime : IAsyncLifetime
{
    private readonly HomeAssistantContainer _homeassistant = new HomeAssistantContainerBuilder()
        .WithResourceMapping(new DirectoryInfo("./HA/config"), "/config")
        .WithVersion(Environment.GetEnvironmentVariable("HomeAssistantVersion") ?? HomeAssistantContainerBuilder.DefaultVersion)
        .Build();
    
    public string? AccessToken;
    public ushort Port => _homeassistant.Port;

    public async Task InitializeAsync()
    {
        await _homeassistant.StartAsync();

        var authorizeResult = await _homeassistant.DoOnboarding();
        AccessToken = (await _homeassistant.GenerateApiToken(authorizeResult.AuthCode)).AccessToken;
    }

    public async Task DisposeAsync()
    {
        await _homeassistant.StopAsync();
    }
}