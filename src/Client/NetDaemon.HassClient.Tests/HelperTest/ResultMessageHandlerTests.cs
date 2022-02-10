namespace NetDaemon.HassClient.Tests.HelperTest;

public class ResultMessageHandlerTests
{
    private readonly Mock<ILogger<ResultMessageHandler>> _loggerMock = new();
    private readonly ResultMessageHandler _resultMessageHandler;

    public ResultMessageHandlerTests()
    {
        _resultMessageHandler = new ResultMessageHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task TestTaskCompleteWithoutLog()
    {
        var task = SomeSuccessfulResult();
        _resultMessageHandler.HandleResult(task, new CommandMessage {Type = "test"});
        await _resultMessageHandler.DisposeAsync().ConfigureAwait(false);
        task.IsCompletedSuccessfully.Should().BeTrue();
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)!), Times.Never());
    }


    [Fact]
    public async Task TestTaskCompleteWithErrorResultShouldLogError()
    {
        var task = SomeUnSuccessfulResult();
        _resultMessageHandler.HandleResult(task, new CommandMessage {Type = "test"});
        await _resultMessageHandler.DisposeAsync().ConfigureAwait(false);
        task.IsCompletedSuccessfully.Should().BeTrue();
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(n => n == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)!), Times.Once());
    }

    [Fact]
    public async Task TestTaskCompleteWithTimeoutShouldLogWarning()
    {
        _resultMessageHandler.WaitForResultTimeout = 1;

        var task = SomeSuccessfulResult();
        _resultMessageHandler.HandleResult(task, new CommandMessage {Type = "test"});
        await _resultMessageHandler.DisposeAsync().ConfigureAwait(false);
        task.IsCompletedSuccessfully.Should().BeTrue();

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(n => n == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)!), Times.Once());
    }

    [Fact]
    public async Task TestTaskCompleteWithTimeoutAndThenErrorShouldLogWarningTwice()
    {
        _resultMessageHandler.WaitForResultTimeout = 1;

        var task = SomeUnSuccessfulResult();
        _resultMessageHandler.HandleResult(task, new CommandMessage {Type = "test"});
        await _resultMessageHandler.DisposeAsync().ConfigureAwait(false);
        task.IsCompletedSuccessfully.Should().BeTrue();

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(n => n == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)!), Times.Exactly(2));
    }

    private async Task<HassMessage> SomeSuccessfulResult()
    {
        // Simulate som time
        await Task.Delay(100);
        return new HassMessage {Success = true};
    }

    private async Task<HassMessage> SomeUnSuccessfulResult()
    {
        // Simulate som time
        await Task.Delay(100);
        return new HassMessage {Success = false};
    }
}
