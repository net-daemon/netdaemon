namespace NetDaemon.HassClient.Tests.Integration;

public class IntegrationTestBase : IClassFixture<HomeAssistantServiceFixture>
{
    protected readonly CancellationTokenSource TokenSource = new(TestSettings.DefaultTimeout);

    protected IntegrationTestBase(HomeAssistantServiceFixture fixture)
    {
        HaFixture = fixture;
    }

    protected HomeAssistantServiceFixture HaFixture { get; }

    /// <summary>
    ///     Returns a connection Home Assistant instance
    /// </summary>
    /// <param name="haSettings">Provide custom setting</param>
    internal async Task<TestContext> GetConnectedClientContext(HomeAssistantSettings? haSettings = null)
    {
        var mock = HaFixture.HaMock ?? throw new ApplicationException("Unexpected for the mock server to be null");

        var loggerClient = new Mock<ILogger<HomeAssistantClient>>();
        var loggerTransport = new Mock<ILogger<IWebSocketClientTransportPipeline>>();
        var loggerConnection = new Mock<ILogger<IHomeAssistantConnection>>();

        var settings = haSettings ?? new HomeAssistantSettings
        {
            Host = "127.0.0.1",
            Port = mock.ServerPort,
            Ssl = false,
            Token = "ABCDEFGHIJKLMNOPQ"
        };

        var appSettingsOptions = Options.Create(settings);

        var client = new HomeAssistantClient(
            loggerClient.Object,
            new WebSocketClientFactory(),
            new WebSocketClientTransportPipelineFactory(loggerTransport.Object),
            new HomeAssistantConnectionFactory(
                loggerConnection.Object,
                new HomeAssistantApiManager(
                    appSettingsOptions,
                    (mock?.HomeAssistantHost.Services.GetRequiredService<IHttpClientFactory>() ??
                     throw new NullReferenceException())
                    .CreateClient()
                )
            )
        );
        var connection = await client.ConnectAsync(
            settings.Host,
            settings.Port,
            settings.Ssl,
            settings.Token,
            TokenSource.Token
        ).ConfigureAwait(false);

        return new TestContext
        {
            HomeAssistantLogger = loggerClient,
            TransportPipelineLogger = loggerTransport,
            HomeAssistantConnectionLogger = loggerConnection,
            HomeAssistantConnction = connection
        };
    }
}