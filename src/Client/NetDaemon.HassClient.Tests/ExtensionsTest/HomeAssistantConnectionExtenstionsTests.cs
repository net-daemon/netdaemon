using NetDaemon.HassClient.Tests.HomeAssistantClientTest;
using NetDaemon.HassClient.Tests.Net;

namespace NetDaemon.HassClient.Tests.ExtensionsTest;


public class HomeAssistantConnectionExtensionsTests
{
    private readonly TransportPipelineMock _pipeline = new();
    private readonly WebSocketClientMock _wsMock = new();

    public HomeAssistantConnectionExtensionsTests()
    {
        _wsMock.Setup(n => n.State).Returns(WebSocketState.Open);
        _pipeline.Setup(n => n.WebSocketState).Returns(WebSocketState.Open);
    }

    private HomeAssistantConnection GetDefaultHomeAssistantClient()
    {
        var loggerMock = new Mock<ILogger<HomeAssistantConnection>>();
        var wsClientFactoryMock = new Mock<IWebSocketClientFactory>();
        var transportPipelineFactoryMock = new Mock<IWebSocketClientTransportPipelineFactory>();
        var apiManagerMock = new Mock<IHomeAssistantApiManager>();

        wsClientFactoryMock.Setup(n => n.New()).Returns(_wsMock.Object);
        transportPipelineFactoryMock.Setup(n => n.New(It.IsAny<IWebSocketClient>())).Returns(_pipeline.Object);

        return new HomeAssistantConnection(loggerMock.Object, _pipeline.Object, apiManagerMock.Object);
    }

    [Fact]
    public async Task TestCancelledTokenShouldReturnNull()
    {
        var connection = GetDefaultHomeAssistantClient();

        var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        (await connection.GetStatesAsync(cancellationTokenSource.Token).ConfigureAwait(false)).Should().BeNull();
        // This should not throw an exception
        await connection.CallServiceAsync("domain", "service", cancelToken: cancellationTokenSource.Token)
            .ConfigureAwait(false);
        await connection.CallServiceWithResponseAsync("domain", "service", cancelToken: cancellationTokenSource.Token)
            .ConfigureAwait(false);
        (await connection.GetServicesAsync(cancellationTokenSource.Token).ConfigureAwait(false)).Should().BeNull();
        (await connection.GetLabelsAsync(cancellationTokenSource.Token).ConfigureAwait(false)).Should().BeNull();
        (await connection.GetConfigAsync(cancellationTokenSource.Token).ConfigureAwait(false)).Should().BeNull();
        (await connection.GetAreasAsync(cancellationTokenSource.Token).ConfigureAwait(false)).Should().BeNull();
        (await connection.GetFloorsAsync(cancellationTokenSource.Token).ConfigureAwait(false)).Should().BeNull();
        (await connection.PingAsync(TimeSpan.FromSeconds(1), cancellationTokenSource.Token).ConfigureAwait(false)).Should().BeNull();
        (await connection.SubscribeToHomeAssistantEventsAsync(null, cancellationTokenSource.Token).ConfigureAwait(false)).Should().BeNull();
        // This should not throw an exception
        await connection.UnsubscribeEventsAsync(0, cancellationTokenSource.Token).ConfigureAwait(false);

    }
}
