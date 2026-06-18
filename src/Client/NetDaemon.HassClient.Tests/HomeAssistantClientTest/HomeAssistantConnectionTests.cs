namespace NetDaemon.HassClient.Tests.HomeAssistantClientTest;

public record FakeCommand : CommandMessage {}

public class HomeAssistantConnectionTests
{
    private static IHomeAssistantConnection GetDefaultHomeAssistantConnection()
    {
        return CreateHomeAssistantConnection(new TransportPipelineMock());
    }

    private static HomeAssistantConnection CreateHomeAssistantConnection(TransportPipelineMock pipeline)
    {
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

    [Fact]
    public async Task HomeAssistantConnectionDisposedMultipleTimesShouldNotThrow()
    {
        var homeAssistantConnection = GetDefaultHomeAssistantConnection();
        await homeAssistantConnection!.DisposeAsync();
        await homeAssistantConnection!.DisposeAsync();
    }

    [Fact]
    public async Task ConcurrentCommandsShouldCompleteFromMatchingResultIds()
    {
        var pipeline = new TransportPipelineMock();
        var sentCommandIds = Channel.CreateUnbounded<int>();
        pipeline
            .Setup(n => n.SendMessageAsync(It.IsAny<FakeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<FakeCommand, CancellationToken>((command, _) =>
            {
                sentCommandIds.Writer.TryWrite(command.Id);
                return Task.CompletedTask;
            });

        await using var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);
        var commands = Enumerable.Range(0, 3).Select(_ => new FakeCommand()).ToArray();
        var resultTasks = commands
            .Select(command => homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(command, CancellationToken.None))
            .ToArray();

        for (var i = 0; i < commands.Length; i++)
        {
            _ = await sentCommandIds.Reader.ReadAsync(CancellationToken.None);
        }

        foreach (var command in commands.Reverse())
        {
            pipeline.AddResponse(new HassMessage { Type = "result", Id = command.Id, Success = true });
        }

        var results = await Task.WhenAll(resultTasks).WaitAsync(TimeSpan.FromSeconds(5));

        results.Select(result => result!.Id).Should().Equal(commands.Select(command => command.Id));
    }

    [Fact]
    public async Task ResultMessagesShouldStillBePublishedToRawHassMessageSubscribers()
    {
        var pipeline = new TransportPipelineMock();
        pipeline
            .Setup(n => n.SendMessageAsync(It.IsAny<FakeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);
        var rawMessages = (homeAssistantConnection as IHomeAssistantHassMessages)!;
        var rawResultTask = rawMessages.OnHassMessage
            .Where(message => message.Type == "result")
            .FirstAsync()
            .ToTask();

        var command = new FakeCommand();
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(command, CancellationToken.None);

        pipeline.AddResponse(new HassMessage { Type = "result", Id = command.Id, Success = true });

        var result = await resultTask.WaitAsync(TimeSpan.FromSeconds(5));
        var rawResult = await rawResultTask.WaitAsync(TimeSpan.FromSeconds(5));

        result!.Id.Should().Be(command.Id);
        rawResult.Id.Should().Be(command.Id);
    }

    [Fact]
    public async Task SendFailureShouldFaultResultWaiterWithoutBreakingLaterCommands()
    {
        var pipeline = new TransportPipelineMock();
        var firstSend = true;
        pipeline
            .Setup(n => n.SendMessageAsync(It.IsAny<FakeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<FakeCommand, CancellationToken>((_, _) =>
            {
                if (!firstSend)
                    return Task.CompletedTask;

                firstSend = false;
                throw new InvalidOperationException("send failed");
            });

        await using var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(new FakeCommand(), CancellationToken.None));

        var command = new FakeCommand();
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(command, CancellationToken.None);
        pipeline.AddResponse(new HassMessage { Type = "result", Id = command.Id, Success = true });

        var result = await resultTask.WaitAsync(TimeSpan.FromSeconds(5));

        result!.Id.Should().Be(command.Id);
    }

    [Fact]
    public async Task DisposingConnectionShouldCancelPendingResultWaiters()
    {
        var pipeline = new TransportPipelineMock();
        pipeline
            .Setup(n => n.SendMessageAsync(It.IsAny<FakeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(new FakeCommand(), CancellationToken.None);

        await homeAssistantConnection.DisposeAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => resultTask.WaitAsync(TimeSpan.FromSeconds(5)));
    }
}
