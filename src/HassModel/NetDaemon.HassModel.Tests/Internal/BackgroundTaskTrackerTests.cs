using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.Internal;

public sealed class BackgroundTaskTrackerTests : IAsyncDisposable
{
    private readonly BackgroundTaskTracker _backgroundTaskTracker;
    private readonly Mock<ILogger<BackgroundTaskTracker>> _loggerMock = new();

    public BackgroundTaskTrackerTests()
    {
        _backgroundTaskTracker = new BackgroundTaskTracker(_loggerMock.Object);
    }

    [Fact]
    public async Task TestBackgroundTaskNormalNotLogError()
    {
        bool isCalled;

        Task CallMe()
        {
            isCalled = true;
            return Task.CompletedTask;
        }

        var timedCancellationTokenSource = new CancellationTokenSource(5000);

        _backgroundTaskTracker.TrackBackgroundTask(CallMe());

        var task = _backgroundTaskTracker.BackgroundTasks.FirstOrDefault();

        if (task.Key is not null)
            // We still have a task in queue so wait for it max 5000 ms
            await task.Key.WaitAsync(timedCancellationTokenSource.Token);

        isCalled.Should().BeTrue();
    }

    [Fact]
    public async Task TestBackgroundTaskWillGetAwaitedWhenDisposed()
    {
        var taskSource1 = new TaskCompletionSource<bool>();
        var taskSource2 = new TaskCompletionSource<bool>();
        var taskSource3 = new TaskCompletionSource<bool>();

        _backgroundTaskTracker.TrackBackgroundTask(taskSource1.Task);
        _backgroundTaskTracker.TrackBackgroundTask(taskSource2.Task);
        _backgroundTaskTracker.TrackBackgroundTask(taskSource3.Task);

        _backgroundTaskTracker.BackgroundTasks.Count.Should().Be(3);

        var disposeTask = _backgroundTaskTracker.DisposeAsync();

        taskSource1.SetResult(true);
        taskSource2.SetResult(true);
        taskSource3.SetResult(true);

        await disposeTask;

        _backgroundTaskTracker.BackgroundTasks.Count.Should().Be(0);
    }

    [Fact]
    public async Task TestDisposeWillTimeoutAfterDefaultWaittime()
    {
        var taskSource = new TaskCompletionSource<bool>();

        _backgroundTaskTracker.TrackBackgroundTask(taskSource.Task);

        _backgroundTaskTracker.BackgroundTasks.Count.Should().Be(1);

        await _backgroundTaskTracker.DisposeAsync();
        // It should still be running after the flush
        _backgroundTaskTracker.BackgroundTasks.Count.Should().Be(1);

        // Make sure we cancel it before leaving the tests
        taskSource.SetResult(true);
    }

    [Fact]
    public async Task TestBackgroundTaskThrowsExceptionWillLogError()
    {
#pragma warning disable CS1998
        async Task CallMeAndIThrowError()
#pragma warning restore CS1998
        {
            throw new InvalidOperationException("Test exception");
        }

        var timedCancellationTokenSource = new CancellationTokenSource(5000);

        _backgroundTaskTracker.TrackBackgroundTask(CallMeAndIThrowError());

        var task = _backgroundTaskTracker.BackgroundTasks.FirstOrDefault();

        if (task.Key is not null)
            // We still have a task in queue so wait for it max 5000 ms
            await task.Key.WaitAsync(timedCancellationTokenSource.Token);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)!), Times.Once);
    }

    public async ValueTask DisposeAsync()
    {
        await _backgroundTaskTracker.DisposeAsync();
    }
}
