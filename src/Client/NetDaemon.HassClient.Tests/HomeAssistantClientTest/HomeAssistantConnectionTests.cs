using System.Reflection;
using Microsoft.Extensions.Time.Testing;

namespace NetDaemon.HassClient.Tests.HomeAssistantClientTest;

public record FakeCommand : CommandMessage {}

public class HomeAssistantConnectionTests
{
    private static IHomeAssistantConnection GetDefaultHomeAssistantConnection()
    {
        return CreateHomeAssistantConnection(new TransportPipelineMock());
    }

    private static HomeAssistantConnection CreateHomeAssistantConnection(TransportPipelineMock pipeline,
        TimeProvider? timeProvider = null,
        TimeSpan? waitForResultTimeout = null)
    {
        pipeline.Setup(n => n.WebSocketState).Returns(WebSocketState.Open);
        var apiManagerMock = new Mock<IHomeAssistantApiManager>();
        var loggerMock = new Mock<ILogger<IHomeAssistantConnection>>();
        return new HomeAssistantConnection(loggerMock.Object, pipeline.Object, apiManagerMock.Object,
            timeProvider, waitForResultTimeout);
    }

    private static int GetPendingResultCount(HomeAssistantConnection connection)
    {
        var pendingResults = typeof(HomeAssistantConnection)
            .GetField("_pendingResults", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(connection);

        return ((System.Collections.ICollection)pendingResults!).Count;
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
    public async Task ResultMessageWithoutPendingCommandShouldStillBePublishedToRawSubscribers()
    {
        var pipeline = new TransportPipelineMock();

        await using var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);
        var rawMessages = (homeAssistantConnection as IHomeAssistantHassMessages)!;
        var rawResultTask = rawMessages.OnHassMessage
            .Where(message => message.Type == "result")
            .FirstAsync()
            .ToTask();

        pipeline.AddResponse(new HassMessage { Type = "result", Id = 123, Success = true });

        var rawResult = await rawResultTask.WaitAsync(TimeSpan.FromSeconds(5));

        rawResult.Id.Should().Be(123);
    }

    [Fact]
    public async Task NonResultMessageShouldBypassPendingResultCompletionAndStillBePublished()
    {
        var pipeline = new TransportPipelineMock();
        pipeline
            .Setup(n => n.SendMessageAsync(It.IsAny<FakeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);
        var rawMessages = (homeAssistantConnection as IHomeAssistantHassMessages)!;
        var rawEventTask = rawMessages.OnHassMessage
            .Where(message => message.Type == "event")
            .FirstAsync()
            .ToTask();

        var command = new FakeCommand();
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(command, CancellationToken.None);

        pipeline.AddResponse(new HassMessage { Type = "event", Id = command.Id });
        var rawEvent = await rawEventTask.WaitAsync(TimeSpan.FromSeconds(5));

        rawEvent.Id.Should().Be(command.Id);
        resultTask.IsCompleted.Should().BeFalse();

        pipeline.AddResponse(new HassMessage { Type = "result", Id = command.Id, Success = true });
        var result = await resultTask.WaitAsync(TimeSpan.FromSeconds(5));

        result!.Id.Should().Be(command.Id);
    }

    [Fact]
    public async Task TimedOutResultWaitShouldRemovePendingResult()
    {
        var pipeline = new TransportPipelineMock();
        var fakeTimeProvider = new FakeTimeProvider();
        var sentCommandIds = Channel.CreateUnbounded<int>();
        pipeline
            .Setup(n => n.SendMessageAsync(It.IsAny<FakeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<FakeCommand, CancellationToken>((command, _) =>
            {
                sentCommandIds.Writer.TryWrite(command.Id);
                return Task.CompletedTask;
            });

        await using var homeAssistantConnection = CreateHomeAssistantConnection(
            pipeline,
            fakeTimeProvider,
            TimeSpan.FromSeconds(20));
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(
            new FakeCommand(),
            CancellationToken.None);

        _ = await sentCommandIds.Reader.ReadAsync(CancellationToken.None);
        GetPendingResultCount(homeAssistantConnection).Should().Be(1);

        fakeTimeProvider.Advance(TimeSpan.FromSeconds(21));

        await Assert.ThrowsAsync<TimeoutException>(async () => await resultTask);
        GetPendingResultCount(homeAssistantConnection).Should().Be(0);
    }

    [Fact]
    public async Task CancelledResultWaitShouldRemovePendingResult()
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

        using var cancelSource = new CancellationTokenSource();
        await using var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(
            new FakeCommand(),
            cancelSource.Token);

        _ = await sentCommandIds.Reader.ReadAsync(CancellationToken.None);
        GetPendingResultCount(homeAssistantConnection).Should().Be(1);

        await cancelSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await resultTask);
        GetPendingResultCount(homeAssistantConnection).Should().Be(0);
    }

    [Fact]
    public async Task SendFailureShouldRemovePendingResultWithoutBreakingLaterCommands()
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
        GetPendingResultCount(homeAssistantConnection).Should().Be(0);

        var command = new FakeCommand();
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(command, CancellationToken.None);
        pipeline.AddResponse(new HassMessage { Type = "result", Id = command.Id, Success = true });

        var result = await resultTask.WaitAsync(TimeSpan.FromSeconds(5));

        result!.Id.Should().Be(command.Id);
    }

    [Fact]
    public async Task SendCancellationShouldCancelResultWaiterWithoutBreakingLaterCommands()
    {
        var pipeline = new TransportPipelineMock();
        var firstSend = true;
        using var cancelSource = new CancellationTokenSource();
        pipeline
            .Setup(n => n.SendMessageAsync(It.IsAny<FakeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<FakeCommand, CancellationToken>(async (_, _) =>
            {
                if (!firstSend)
                    return;

                firstSend = false;
                await cancelSource.CancelAsync();
                await Task.FromCanceled(cancelSource.Token);
            });

        await using var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(new FakeCommand(), cancelSource.Token));
        GetPendingResultCount(homeAssistantConnection).Should().Be(0);

        var command = new FakeCommand();
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(command, CancellationToken.None);
        pipeline.AddResponse(new HassMessage { Type = "result", Id = command.Id, Success = true });

        var result = await resultTask.WaitAsync(TimeSpan.FromSeconds(5));

        result!.Id.Should().Be(command.Id);
    }

    [Fact]
    public async Task FailedResultMessageShouldCompletePendingResultThenThrow()
    {
        var pipeline = new TransportPipelineMock();
        pipeline
            .Setup(n => n.SendMessageAsync(It.IsAny<FakeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await using var homeAssistantConnection = CreateHomeAssistantConnection(pipeline);
        var command = new FakeCommand { Type = "test_command" };
        var resultTask = homeAssistantConnection.SendCommandAndReturnHassMessageResponseAsync(command, CancellationToken.None);

        pipeline.AddResponse(new HassMessage
        {
            Type = "result",
            Id = command.Id,
            Success = false,
            Error = new HassError { Code = 42, Message = "Unable to do what you asked" }
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => resultTask.WaitAsync(TimeSpan.FromSeconds(5)));
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
