using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using NetDaemon.Extensions.Scheduler;
using Xunit;

namespace NetDaemon.Extensions.Scheduling.Tests;

public class CronTests
{
    [Fact]
    public void TestCron()
    {
        var count = 0;
        var sched = new TestScheduler();

        sched.AdvanceTo(DateTime.UtcNow.Ticks);

        var sub = sched.ScheduleCron("0 * * * *", () => count++);

        sched.AdvanceBy(TimeSpan.FromHours(1).Ticks);
        count.Should().Be(1);

        sched.AdvanceBy(TimeSpan.FromHours(1).Ticks);
        count.Should().Be(2);

        sched.AdvanceBy(TimeSpan.FromHours(1).Ticks);
        count.Should().Be(3);

        sub.Dispose();
        sched.AdvanceBy(TimeSpan.FromHours(1).Ticks);
        count.Should().Be(3, because: "Action should not fire after Dispose()");
    }

    [Fact]
    public void TestCronSeconds()
    {
        var count = 0;
        var sched = new TestScheduler();

        sched = new TestScheduler();
        count = 0;
        sched.AdvanceTo(new DateTime(2020, 2, 3, 8, 0, 28).Ticks);
        var subSec = sched.ScheduleCron("*/30 0 * * * *", () => count++, hasSeconds: true); // 0 and 30 seconds after every whole hour

        sched.AdvanceTo(new DateTime(2020, 2, 3, 8, minute: 0, second: 29).Ticks);
        count.Should().Be(0);

        sched.AdvanceTo(new DateTime(2020, 2, 3, 8, minute: 0, second: 30).Ticks);
        count.Should().Be(1);

        sched.AdvanceTo(new DateTime(2020, 2, 3, 8, minute: 1, second: 1).Ticks);
        count.Should().Be(1);

        sched.AdvanceBy(TimeSpan.FromMinutes(10).Ticks);
        count.Should().Be(1);

        sched.AdvanceTo(new DateTime(2020, 2, 3, 9, minute: 0, second: 0).Ticks);
        count.Should().Be(2);

        sched.AdvanceTo(new DateTime(2020, 2, 3, 9, minute: 0, second: 30).Ticks);
        count.Should().Be(3);

        subSec.Dispose();
        sched.AdvanceBy(TimeSpan.FromHours(1).Ticks);
        count.Should().Be(3, because: "Action should not fire after Dispose()");
    }

    [Fact]
    public void ContinueAfterActionThrows()
    {
        var count = 0;
        var sched = new TestScheduler();
        sched.AdvanceTo(new DateTime(2010, 1, 12, 9, 12, 0, DateTimeKind.Utc).Ticks);

        var sub = sched
            .WrapWithLogger(NullLogger.Instance)
            .ScheduleCron("* * * * *", () =>
        {
            count++;
            throw new Exception("bang!");
        });

        sched.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        count.Should().Be(1);

        sched.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        count.Should().Be(2, because: "Action should still fire after previous Exception");
    }

    [Fact]
    public void TestCronLocalTime()
    {
        using (new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("US/Eastern")))
        {
            var count = 0;
            var sched = new TestScheduler();
            var universalTime = new DateTime(2022, 1, 12, 4, 59, 30, DateTimeKind.Local).ToUniversalTime();
            sched.AdvanceTo(universalTime.Ticks);

            sched.ScheduleCron("0 5 * * *", () => count++);

            sched.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
            count.Should().Be(1, because: "Cron should be interpreted as local time");
        }
    }

    [Fact]
    public void TestCronDSTForwards()
    {
        using (new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam")))
        {
            var count = 0;
            var sched = new TestScheduler();

            var justBeforeDST = new DateTime(2022, 3, 27, 1, 0, 0, DateTimeKind.Local).ToUniversalTime();
            var justAfterDST = new DateTime(2022, 3, 27, 3, 0, 0, DateTimeKind.Local).ToUniversalTime();
            sched.AdvanceTo(justBeforeDST.Ticks);

            sched.ScheduleCron("* * * * *", () => count++);

            sched.AdvanceTo(justAfterDST.Ticks);
            count.Should().Be(60, because: "Only 1 hour has passed between 1 and 3 AM");
        }
    }


    [Fact]
    public void TestCronDSTBackwards()
    {
        using (new FakeLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam")))
        {
            var count = 0;
            var sched = new TestScheduler();
            var justBeforeDST = new DateTime(2022, 10, 30, 1, 0, 0, DateTimeKind.Local).ToUniversalTime();
            var justAfterDST =  new DateTime(2022, 10, 30, 3, 0, 0, DateTimeKind.Local).ToUniversalTime();

            sched.AdvanceTo(justBeforeDST.Ticks);

            sched.ScheduleCron("* * * * *", () => count++);

            sched.AdvanceTo(justAfterDST.Ticks);
            count.Should().Be(3 * 60, because: "3 hours have passed between 1 and 3 AM");
        }
    }


    [Fact]
    public void AllExceptionsAreLogged()
    {
        // ARRANGE
        var testScheduler = new TestScheduler();
        var loggerMock = new Mock<ILogger<NetDaemonScheduler>>();
        // sets the date to a specific time so we do not get errors in UTC
        var dueTime = new DateTime(2021, 1, 1, 0, 0, 0);
        testScheduler.AdvanceTo(dueTime.Ticks);

        var netDaemonScheduler = new DisposableScheduler(testScheduler).WrapWithLogger(loggerMock.Object);

        netDaemonScheduler.ScheduleCron("* * * * *", () => throw new InvalidOperationException("hello"));

        // ACT and ASSERT
        testScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        // ASSERT that error is logged once
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((_, _) => true)!), Times.Exactly(5));
    }
}
