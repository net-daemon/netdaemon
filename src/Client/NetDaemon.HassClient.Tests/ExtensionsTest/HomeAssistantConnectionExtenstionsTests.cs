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

        await Assert.ThrowsAsync<OperationCanceledException>(()=> connection.GetStatesAsync(cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=>
                connection.CallServiceAsync("domain", "service", cancelToken: cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=>
                connection.CallServiceWithResponseAsync("domain", "service", cancelToken: cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=> connection.GetServicesAsync(cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=> connection.GetLabelsAsync(cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=> connection.GetAreasAsync(cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=> connection.GetFloorsAsync(cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=> connection.GetConfigAsync(cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=> connection.PingAsync(TimeSpan.FromSeconds(1), cancellationTokenSource.Token));
        await Assert.ThrowsAsync<OperationCanceledException>(()=> connection.UnsubscribeEventsAsync(0, cancellationTokenSource.Token));

        try
        {
            // Assert.ThrowsAsync could not be used on this function
            await connection.SubscribeToHomeAssistantEventsAsync(null, cancellationTokenSource.Token);
            Assert.Fail("Should throw exception");
        }
        catch (OperationCanceledException)
        {
            // This should throw an exception
        }

    }
}
