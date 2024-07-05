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
    public async Task TestBackgroundTaskWillGetFlushedAndCanelled()
    {
        static async Task CallMe(CancellationToken cancellationToken)
        {
            await Task.Delay(5000, cancellationToken);
        }

        using var timedCancellationTokenSource = new CancellationTokenSource(5000);

        _backgroundTaskTracker.TrackBackgroundTask(CallMe(timedCancellationTokenSource.Token));
        _backgroundTaskTracker.TrackBackgroundTask(CallMe(timedCancellationTokenSource.Token));
        _backgroundTaskTracker.TrackBackgroundTask(CallMe(timedCancellationTokenSource.Token));

        _backgroundTaskTracker.BackgroundTasks.Count.Should().Be(3);

        var flushTask = _backgroundTaskTracker.Flush();

        timedCancellationTokenSource.CancelAfter(500);

        await flushTask;

        _backgroundTaskTracker.BackgroundTasks.Count.Should().Be(0);
    }

    [Fact]
    public async Task TestFlushWillTimeoutAfterDefaultWaittime()
    {
        var isCalled = false;

        async Task CallMe(CancellationToken cancellationToken)
        {
            await Task.Delay(25000, cancellationToken);
            isCalled = true;
        }

        using var timedCancellationTokenSource = new CancellationTokenSource(25000);

        _backgroundTaskTracker.TrackBackgroundTask(CallMe(timedCancellationTokenSource.Token));

        _backgroundTaskTracker.BackgroundTasks.Count.Should().Be(1);

        await _backgroundTaskTracker.Flush();

        _backgroundTaskTracker.BackgroundTasks.Count.Should().Be(0);
        isCalled.Should().BeFalse();

        await timedCancellationTokenSource.CancelAsync();
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
