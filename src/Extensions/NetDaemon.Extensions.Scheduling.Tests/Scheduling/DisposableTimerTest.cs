using NetDaemon.Extensions.Scheduler;
using Xunit;

namespace NetDaemon.Extensions.Scheduling.Tests;

public class DisposableTimerTest
{
    [Fact]
    public void DisposeTwice_NoException()
    {
        var timer = new DisposableTimer(CancellationToken.None);
        
        timer.Dispose();
        timer.Dispose();
    }
}