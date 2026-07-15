using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Formatter;
using NetDaemon.Tests.Integration.Helpers.HomeAssistantTestContainer;
using System.Text;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

/// <summary>
/// Manages the shared Home Assistant and MQTT broker containers for integration tests.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeAssistantLifetime"/> class.
    /// </summary>
    public HomeAssistantLifetime()
    {
        _mqttBroker = new ContainerBuilder()
            .WithImage(Environment.GetEnvironmentVariable("MqttBrokerImage") ?? "eclipse-mosquitto:2")
            .WithNetwork(_network)
            .WithNetworkAliases(MqttContainerAlias)
            .WithPortBinding(MqttContainerPort, true)
            .WithResourceMapping(Encoding.UTF8.GetBytes(MosquittoConfiguration), "/mosquitto/config/mosquitto.conf")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilInternalTcpPortIsAvailable(MqttContainerPort)
                    .UntilExternalTcpPortIsAvailable(MqttContainerPort))
            .WithLogger(_loggerFactory.CreateLogger("MqttBrokerContainer"))
            .Build();

        _homeassistant = new HomeAssistantContainerBuilder()
            .WithNetwork(_network)
            .WithResourceMapping(new DirectoryInfo("./HA/config"), "/config")
            .WithLogger(_loggerFactory.CreateLogger<HomeAssistantContainer>())
            .WithVersion(Environment.GetEnvironmentVariable("HomeAssistantVersion") ?? HomeAssistantContainerBuilder.DefaultVersion)
            .Build();
    }

    /// <summary>
    /// Gets or sets the Home Assistant API access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets the mapped Home Assistant HTTP port.
    /// </summary>
    public ushort Port => _homeassistant.Port;

    /// <summary>
    /// Gets the host name used by the test host to connect to the MQTT broker.
    /// </summary>
    public string MqttHost { get; } = "localhost";

    /// <summary>
    /// Gets the mapped MQTT broker port.
    /// </summary>
    public ushort MqttPort => _mqttBroker.GetMappedPublicPort(MqttContainerPort);

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _network.CreateAsync();
        await _mqttBroker.StartAsync();
        await WaitForMqttBrokerAsync();
        await _homeassistant.StartAsync();

        var authorizeResult = await _homeassistant.DoOnboarding();
        AccessToken = (await _homeassistant.GenerateApiToken(authorizeResult.AuthCode)).AccessToken;
        await _homeassistant.AddIntegrations(AccessToken, new MqttBrokerSettings(MqttContainerAlias, MqttContainerPort));
    }

    /// <inheritdoc />
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

    private async Task WaitForMqttBrokerAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var logger = _loggerFactory.CreateLogger<HomeAssistantLifetime>();
        Exception? lastException = null;

        while (!timeout.IsCancellationRequested)
        {
            try
            {
                using var client = new MqttClientFactory().CreateMqttClient();
                var clientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(MqttHost, MqttPort)
                    .WithProtocolVersion(MqttProtocolVersion.V311)
                    .WithClientId($"netdaemon-test-readiness-{Guid.NewGuid():N}")
                    .Build();

                var connectResult = await client.ConnectAsync(clientOptions, timeout.Token);
                if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
                {
                    await client.DisconnectAsync();
                    return;
                }

                lastException = new InvalidOperationException($"MQTT broker rejected readiness connection: {connectResult.ResultCode} {connectResult.ReasonString}");
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }

            await DelayNextMqttBrokerAttempt(timeout.Token);
        }

        logger.LogError(lastException, "MQTT broker endpoint {Host}:{Port} did not become ready within 30 seconds", MqttHost, MqttPort);
        throw new TimeoutException($"MQTT broker endpoint {MqttHost}:{MqttPort} did not become ready within 30 seconds.", lastException);
    }

    private static async Task DelayNextMqttBrokerAttempt(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
