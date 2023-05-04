using NetDaemon.HassClient.Tests.Net;

namespace NetDaemon.HassClient.Tests.HomeAssistantClientTest;

public class HomeAssistantClientTests
{
    private readonly HomeAssistantConnectionMock _haConnectionMock = new();
    private readonly TransportPipelineMock _pipeline = new();
    private readonly WebSocketClientMock _wsMock = new();

    /// <summary>
    ///     Return a mocked Home Assistant Client
    /// </summary>
    private HomeAssistantClient GetDefaultHomeAssistantClient()
    {
        var connFactoryMock = new Mock<IHomeAssistantConnectionFactory>();
        var loggerMock = new Mock<ILogger<HomeAssistantClient>>();
        var wsClientFactoryMock = new Mock<IWebSocketClientFactory>();
        var transportPipelineFactoryMock = new Mock<IWebSocketClientTransportPipelineFactory>();

        wsClientFactoryMock.Setup(n => n.New()).Returns(_wsMock.Object);
        transportPipelineFactoryMock.Setup(n => n.New(It.IsAny<IWebSocketClient>())).Returns(_pipeline.Object);
        connFactoryMock.Setup(n =>
            n.New(It.IsAny<IWebSocketClientTransportPipeline>())).Returns(_haConnectionMock.Object);
        return new HomeAssistantClient(
            loggerMock.Object,
            wsClientFactoryMock.Object,
            transportPipelineFactoryMock.Object,
            connFactoryMock.Object);
    }

    [Fact]
    public async Task TestConnectWithHomeAShouldReturnConnection()
    {
        var client = GetDefaultConnectOkHomeAssistantClient();

        var connection = await client.ConnectAsync("host", 1, true, "token", "path", CancellationToken.None)
            .ConfigureAwait(false);

        connection.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TestConnectWithOldVersionHomeAShouldReturnConnection()
    {
        var client = GetDefaultConnectOkToOldVersionHomeAssistantClient();

        var connection = await client.ConnectAsync("host", 1, true, "token", "path", CancellationToken.None)
            .ConfigureAwait(false);

        connection.Should().NotBeNull();
    }

    [Fact]
    public async Task TestConnectWithHomeAssistantNotReadyShouldThrowException()
    {
        var client = GetDefaultAuthorizedHomeAssistantClient();

        _haConnectionMock.AddConfigResponseMessage(
            new HassConfig
            {
                State = "ANY_STATE_BUT_RUNNING"
            }
        );

        await Assert.ThrowsAsync<HomeAssistantConnectionException>(async () =>
            await client.ConnectAsync("host", 1, true, "token", "path", CancellationToken.None).ConfigureAwait(false));
    }

    [Fact]
    public void TestInstanceNewConnectionOnClosedWebsocketThrowsExceptionShouldThrowException()
    {
        _pipeline.SetupGet(
            n => n.WebSocketState
        ).Returns(WebSocketState.Closed);
        var loggerMock = new Mock<ILogger<IHomeAssistantConnection>>();
        Assert.Throws<ApplicationException>(() =>
            _ = new HomeAssistantConnection(
                loggerMock.Object,
                _pipeline.Object,
                new Mock<IHomeAssistantApiManager>().Object)
                );
    }

    /// <summary>
    ///     Return a pre-authenticated OK HomeAssistantClient
    /// </summary>
    private HomeAssistantClient GetDefaultAuthorizedHomeAssistantClient()
    {
        // First add the authorization responses from pipeline
        _pipeline.AddResponse(
            new HassMessage
            {
                Type = "auth_required"
            }
        );
        _pipeline.AddAuthResponse(
            new HassAuthResponse()
            {
                Type = "auth_ok",
                HaVersion = "2023.1.0"
            }
        );
        _pipeline.AddResponse(
            new HassMessage
            {
                Id = 1,
                Type = "result",
                Success = true
            } //{"id":1,"type":"result","success":true,"result":null}
        );
        return GetDefaultHomeAssistantClient();
    }
    
    /// <summary>
    ///     Return a pre authenticated and running state
    ///     HomeAssistantClient
    /// </summary>
    private HomeAssistantClient GetDefaultConnectOkHomeAssistantClient()
    {
        // For a successful connection we need success on authorization
        // and success on getting a config message that has state="RUNNING"

        // First add the authorization responses from pipeline
        _pipeline.AddResponse(
            new HassMessage
            {
                Type = "auth_required"
            }
        );
        _pipeline.AddAuthResponse(
            new HassAuthResponse()
            {
                Type = "auth_ok",
                HaVersion = "2023.1.0"
            }
        );
        _pipeline.AddResponse(
            new HassMessage
            {
                Id = 1,
                Type = "result",
                Success = true
            } //{"id":1,"type":"result","success":true,"result":null}
        );
        
        // The add the fake config state that says running
        _haConnectionMock.AddConfigResponseMessage(
            new HassConfig
            {
                State = "RUNNING"
            }
        );
        return GetDefaultHomeAssistantClient();
    }    
    
    /// <summary>
    ///     Return a pre authenticated and running state
    ///     HomeAssistantClient with version less than 2023.9.x
    /// </summary>
    private HomeAssistantClient GetDefaultConnectOkToOldVersionHomeAssistantClient()
    {
        // For a successful connection we need success on authorization
        // and success on getting a config message that has state="RUNNING"

        // First add the authorization responses from pipeline
        _pipeline.AddResponse(
            new HassMessage
            {
                Type = "auth_required"
            }
        );
        _pipeline.AddAuthResponse(
            new HassAuthResponse()
            {
                Type = "auth_ok",
                HaVersion = "2022.8.99"
            }
        );
        // The add the fake config state that says running
        _haConnectionMock.AddConfigResponseMessage(
            new HassConfig
            {
                State = "RUNNING"
            }
        );
        return GetDefaultHomeAssistantClient();
    }
}
