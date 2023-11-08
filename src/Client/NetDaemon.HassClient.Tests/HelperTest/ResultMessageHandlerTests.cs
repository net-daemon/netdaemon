using Microsoft.Extensions.Time.Testing;

namespace NetDaemon.HassClient.Tests.HelperTest;

public sealed class ResultMessageHandlerTests : IAsyncLifetime, IAsyncDisposable
{
    private readonly Mock<ILogger<ResultMessageHandler>> _loggerMock = new();
    private readonly ResultMessageHandler _resultMessageHandler;
    private readonly FakeTimeProvider _fakeTimeProvider;

    private readonly CallServiceCommand _callServiceCommand = new() { Domain = "light", Service = "turn_on", ServiceData = new {brightness = 2.3 }};
    private readonly string _callServiceCommandJson = """{"domain":"light","service":"turn_on","service_data":{"brightness":2.3},"target":null,"id":0,"type":"call_service"}""";

    public ResultMessageHandlerTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _resultMessageHandler = new ResultMessageHandler(_loggerMock.Object, _fakeTimeProvider);
    }

    [Fact]
    public async Task TestTaskCompleteWithoutLog()
    {
        var tcs = new TaskCompletionSource<HassMessage>();

        _resultMessageHandler.HandleResult(tcs.Task, new CommandMessage {Type = "test"});
        tcs.SetResult(new HassMessage {Success = true});
        await FlushMessageHandler().ConfigureAwait(false);

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
        var tcs = new TaskCompletionSource<HassMessage>();

        _resultMessageHandler.HandleResult(tcs.Task, _callServiceCommand);
        tcs.SetResult(new HassMessage { Success = false, Error = new HassError { Code = 42, Message = "Unable to do what you asked" }});

        await FlushMessageHandler().ConfigureAwait(false);
        _loggerMock.VerifyErrorWasCalled($"Failed command (call_service) error: HassError {{ Code = 42, Message = Unable to do what you asked }}.  Sent command is {_callServiceCommandJson}");
    }

    [Fact]
    public async Task TestTaskCompleteWithExceptionShouldLogError()
    {
        var tcs = new TaskCompletionSource<HassMessage>();
        _resultMessageHandler.HandleResult(tcs.Task, _callServiceCommand);

        tcs.SetException(new InvalidOperationException("Ohh noooo!"));

        await FlushMessageHandler();
        _loggerMock.VerifyErrorWasCalled($"Exception waiting for result message.  Sent command is {_callServiceCommandJson}");
    }

    [Fact]
    public async Task TestTaskCompleteWithTimeoutShouldLogWarning()
    {
        var tcs = new TaskCompletionSource<HassMessage>();
        _resultMessageHandler.HandleResult(tcs.Task, _callServiceCommand);

        // simulate a call that takes one minute
        _fakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        tcs.SetResult(new HassMessage());

        await FlushMessageHandler();
        _loggerMock.VerifyWarningWasCalled($"Command (call_service) did not get response in timely fashion.  Sent command is {_callServiceCommandJson}");
    }


    [Fact]
    public async Task TestTaskCompleteWithTimeoutAndErrorShouldLogTwice()
    {
        var tcs = new TaskCompletionSource<HassMessage>();
        _resultMessageHandler.HandleResult(tcs.Task, _callServiceCommand);

        // simulate a call that takes one minute and then returns an error
        _fakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        tcs.SetResult(new HassMessage { Success = false, Error = new HassError { Code = 42, Message = "Unable to do what you asked" }});

        await FlushMessageHandler();
        _loggerMock.VerifyWarningWasCalled($"Command (call_service) did not get response in timely fashion.  Sent command is {_callServiceCommandJson}");
        _loggerMock.VerifyErrorWasCalled($"Failed command (call_service) error: HassError {{ Code = 42, Message = Unable to do what you asked }}.  Sent command is {_callServiceCommandJson}");
    }

    [Fact]
    public async Task TestTaskCompleteWithTimeoutAndExceptionShouldLogTwice()
    {
        var tcs = new TaskCompletionSource<HassMessage>();
        _resultMessageHandler.HandleResult(tcs.Task, _callServiceCommand);

        // simulate a call that takes one minute and then throws an Exception
        _fakeTimeProvider.Advance(TimeSpan.FromMinutes(1));
        tcs.SetException(new InvalidOperationException("Ohh noooo!"));

        await FlushMessageHandler();
        _loggerMock.VerifyWarningWasCalled($"Command (call_service) did not get response in timely fashion.  Sent command is {_callServiceCommandJson}");
        _loggerMock.VerifyErrorWasCalled($"Exception waiting for result message.  Sent command is {_callServiceCommandJson}");
    }

    async Task FlushMessageHandler()
    {
        await _resultMessageHandler.WaitPendingBackgroundTasksAsync()
            .WaitAsync(TimeSpan.FromMilliseconds(100)) // avoid blocking the test in case something is wrong
            .ConfigureAwait(false);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();

    public async ValueTask DisposeAsync()
    {
        await _resultMessageHandler.DisposeAsync();
    }
}
