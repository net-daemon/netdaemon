namespace NetDaemon.HassClient.Tests.HomeAssistantClientTest;

public record FakeCommand : CommandMessage {}

public class HomeAssistantConnectionTests
{
    private static IHomeAssistantConnection GetDefaultHomeAssistantConnection()
    {
        var pipeline = new TransportPipelineMock();
        pipeline.Setup(n => n.WebSocketState).Returns(WebSocketState.Open);
        var apiManagerMock = new Mock<IHomeAssistantApiManager>();
        var loggerMock = new Mock<ILogger<IHomeAssistantConnection>>();
        return new HomeAssistantConnection(loggerMock.Object, pipeline.Object, apiManagerMock.Object);
    }

    [Fact]
    public async Task UsingDisposedConnectionWhenSendCommandShouldThrowException()
    {
        var homeAssistantConnection = GetDefaultHomeAssistantConnection();
        await homeAssistantConnection!.DisposeAsync();

        Func<Task> act = async () =>
        {
            await homeAssistantConnection!.SendCommandAsync(new FakeCommand(), CancellationToken.None);
        };

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task UsingDisposedConnectionWhenGetApiCommandShouldThrowException()
    {
        var homeAssistantConnection = GetDefaultHomeAssistantConnection();
        await homeAssistantConnection!.DisposeAsync();

        Func<Task> act = async () =>
        {
            await homeAssistantConnection!.GetApiCallAsync<string>("test", CancellationToken.None);
        };

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task UsingDisposedConnectionWhenPostApiShouldThrowException()
    {
        var homeAssistantConnection = GetDefaultHomeAssistantConnection();
        await homeAssistantConnection!.DisposeAsync();

        Func<Task> act = async () =>
        {
            await homeAssistantConnection!.PostApiCallAsync<string>("test", CancellationToken.None, null);
        };

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }
}
