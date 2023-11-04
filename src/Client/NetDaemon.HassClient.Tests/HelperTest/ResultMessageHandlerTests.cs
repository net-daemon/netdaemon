namespace NetDaemon.HassClient.Tests.HelperTest;

public class ResultMessageHandlerTests : IAsyncDisposable
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
    public async Task TestTaskCompleteWithErrorResultShouldLogWarning()
    {
        // TODO: Test sometimes fails in CI
        var task = SomeUnSuccessfulResult();
        _resultMessageHandler.HandleResult(task, new CommandMessage {Type = "test"});
        await _resultMessageHandler.DisposeAsync().ConfigureAwait(false);
        task.IsCompletedSuccessfully.Should().BeTrue();
        _loggerMock.VerifyWarningWasCalled("Failed command (test) error: (null).  Sent command is CommandMessage { Type = test, Id = 0 }");
    }

    [Fact]
    public async Task TestTaskCompleteWithExceptionShouldLogError()
    {
        // TODO: Test sometimes fails in CI
        var task = SomeUnSuccessfulResultThrowsException();
        _resultMessageHandler.HandleResult(task, new CommandMessage {Type = "test"});
        await _resultMessageHandler.DisposeAsync().ConfigureAwait(false);
        task.IsCompletedSuccessfully.Should().BeFalse();
        task.IsFaulted.Should().BeTrue();
        _loggerMock.VerifyErrorWasCalled("Exception waiting for result message  Sent command is CommandMessage { Type = test, Id = 0 }");
    }

    [Fact]
    public async Task TestTaskCompleteWithTimeoutShouldLogWarning()
    {
        _resultMessageHandler.WaitForResultTimeout = 1;

        var task = SomeSuccessfulResult();
        _resultMessageHandler.HandleResult(task, new CommandMessage {Type = "test"});
        await _resultMessageHandler.DisposeAsync().ConfigureAwait(false);
        task.IsCompletedSuccessfully.Should().BeTrue();
        _loggerMock.VerifyWarningWasCalled("Command (test) did not get response in timely fashion.  Sent command is CommandMessage { Type = test, Id = 0 }");
    }



    private static async Task<HassMessage> SomeSuccessfulResult()
    {
        // Simulate som time
        await Task.Delay(400);
        return new HassMessage {Success = true};
    }

    private static async Task<HassMessage> SomeUnSuccessfulResult()
    {
        // Simulate som time
        await Task.Delay(400);
        return new HassMessage {Success = false};
    }

    private static async Task<HassMessage> SomeUnSuccessfulResultThrowsException()
    {
        // Simulate som time
        await Task.Delay(400);
        throw new InvalidOperationException("Ohh noooo!");
    }

    public async ValueTask DisposeAsync()
    {
        await _resultMessageHandler.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
