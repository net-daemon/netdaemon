using Microsoft.Extensions.Logging;
using NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

public class HomeAssistantLifetime : IAsyncLifetime
{
    private readonly HomeAssistantContainer _homeassistant = new HomeAssistantContainerBuilder()
        .WithResourceMapping(new DirectoryInfo("./HA/config"), "/config")
        .WithLogger(LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<HomeAssistantContainer>())
        .WithVersion(Environment.GetEnvironmentVariable("HomeAssistantVersion") ?? HomeAssistantContainerBuilder.DefaultVersion)
        .Build();

    public string? AccessToken { get; set; }
    public ushort Port => _homeassistant.Port;

    public async Task InitializeAsync()
    {
        await _homeassistant.StartAsync();

        var authorizeResult = await _homeassistant.DoOnboarding();
        AccessToken = (await _homeassistant.GenerateApiToken(authorizeResult.AuthCode)).AccessToken;
        await _homeassistant.AddIntegrations(AccessToken);
    }

    public async Task DisposeAsync()
    {
        var (_, stderr) = await _homeassistant.GetLogsAsync();
        Console.WriteLine($"Writing Homeassistant logs to console:{Environment.NewLine}{stderr}{Environment.NewLine}End of Homeassistant logs");
        await _homeassistant.StopAsync();
    }
}

