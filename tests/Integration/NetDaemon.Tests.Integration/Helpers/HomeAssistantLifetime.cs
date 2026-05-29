using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Logging;
using NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;
using System.Text;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

public class HomeAssistantLifetime : IAsyncLifetime
{
    private const int MqttContainerPort = 1883;
    private const string MqttContainerAlias = "mqtt";
    private const string MosquittoConfiguration = """
        listener 1883 0.0.0.0
        allow_anonymous true
        persistence false
        log_dest stdout
        """;

    private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    private readonly INetwork _network = new NetworkBuilder().Build();
    private readonly IContainer _mqttBroker;
    private readonly HomeAssistantContainer _homeassistant;

    public HomeAssistantLifetime()
    {
        _mqttBroker = new ContainerBuilder()
            .WithImage(Environment.GetEnvironmentVariable("MqttBrokerImage") ?? "eclipse-mosquitto:2")
            .WithNetwork(_network)
            .WithNetworkAliases(MqttContainerAlias)
            .WithPortBinding(MqttContainerPort, true)
            .WithResourceMapping(Encoding.UTF8.GetBytes(MosquittoConfiguration), "/mosquitto/config/mosquitto.conf")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(MqttContainerPort))
            .WithLogger(_loggerFactory.CreateLogger("MqttBrokerContainer"))
            .Build();

        _homeassistant = new HomeAssistantContainerBuilder()
            .WithNetwork(_network)
            .WithResourceMapping(new DirectoryInfo("./HA/config"), "/config")
            .WithLogger(_loggerFactory.CreateLogger<HomeAssistantContainer>())
            .WithVersion(Environment.GetEnvironmentVariable("HomeAssistantVersion") ?? HomeAssistantContainerBuilder.DefaultVersion)
            .Build();
    }

    public string? AccessToken { get; set; }
    public ushort Port => _homeassistant.Port;
    public string MqttHost { get; } = "localhost";
    public ushort MqttPort => _mqttBroker.GetMappedPublicPort(MqttContainerPort);

    public async Task InitializeAsync()
    {
        await _network.CreateAsync();
        await _mqttBroker.StartAsync();
        await _homeassistant.StartAsync();

        var authorizeResult = await _homeassistant.DoOnboarding();
        AccessToken = (await _homeassistant.GenerateApiToken(authorizeResult.AuthCode)).AccessToken;
        await _homeassistant.AddIntegrations(AccessToken, new MqttBrokerSettings(MqttContainerAlias, MqttContainerPort));
    }

    public async Task DisposeAsync()
    {
        var (_, homeAssistantStderr) = await _homeassistant.GetLogsAsync();
        Console.WriteLine($"Writing Homeassistant logs to console:{Environment.NewLine}{homeAssistantStderr}{Environment.NewLine}End of Homeassistant logs");

        var (_, mqttStderr) = await _mqttBroker.GetLogsAsync();
        Console.WriteLine($"Writing MQTT broker logs to console:{Environment.NewLine}{mqttStderr}{Environment.NewLine}End of MQTT broker logs");

        await _homeassistant.DisposeAsync();
        await _mqttBroker.DisposeAsync();
        await _network.DisposeAsync();
        _loggerFactory.Dispose();
    }
}
