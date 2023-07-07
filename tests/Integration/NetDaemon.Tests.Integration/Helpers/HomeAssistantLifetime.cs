using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Runtime;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

public class HomeAssistantAuthorizeResult
{
    public string auth_code { get; set; }
}

public class HomeAssistantTokenResult
{
    public string access_token { get; set; }
    public string token_type { get; set; }
    public string refresh_token { get; set; }
    public int expires_in { get; set; }
    public string ha_auth_provider { get; set; }
}

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

public class HomeAssistantLifetime : IAsyncLifetime
{
    private readonly IContainer _homeassistant = new ContainerBuilder()
        .WithImage("homeassistant/home-assistant:stable")
        .WithResourceMapping(new DirectoryInfo("./HA/config"), "/config")
        .WithPortBinding(8123, true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilHttpRequestIsSucceeded(request => request.ForPort(8123).ForPath("/")))
        .Build();

    public int Port => _homeassistant.GetMappedPublicPort(8123);
    public string AccessToken;

    public async Task InitializeAsync()
    {
        await _homeassistant.StartAsync();

        AccessToken = await GenerateAccessToken();
    }

    private async Task<string> GenerateAccessToken()
    {
        var client = new HttpClient()
        {
            BaseAddress = new Uri($"http://localhost:{Port}")
        };

        var clientId = "http://foobar";
        var onboardingResult = await client.PostAsync("/api/onboarding/users", JsonContent.Create(new
        {
            client_id = clientId,
            name = "foobar",
            username = "foobar",
            password = "1",
            language = "en-GB"
        }));

        onboardingResult.EnsureSuccessStatusCode();

        var authorizeResult = await onboardingResult.Content.ReadFromJsonAsync<HomeAssistantAuthorizeResult>();

        var tokenResponse = await client.PostAsync("/auth/token", new FormUrlEncodedContent(
            new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", authorizeResult.auth_code },
                { "client_id", clientId },
            }));

        tokenResponse.EnsureSuccessStatusCode();
        var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<HomeAssistantTokenResult>();
        return tokenResult.access_token;
    }

    public async Task DisposeAsync()
    {
        await _homeassistant.StopAsync();
    }
}