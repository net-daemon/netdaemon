using System.Reactive.Concurrency;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NetDaemon.Extensions.Scheduler;
using Xunit;

namespace NetDaemon.Extensions.Scheduling.Tests;

public class DisposableSchedulerTest
{
    [Fact]
    public void ScheduleRunsAtSpeciiefTime()
    {
        int called = 0;
        var (inner, disposableScheduler) = CreateScheduler();

        disposableScheduler.Schedule(1,  (i, action) => { called++;});
        called.Should().Be(0);
        inner.AdvanceBy(TimeSpan.FromTicks(1).Ticks);

        called.Should().Be(1);
    }

    [Fact]
    public void _SchedulePeriodicStopsAfterDisposeOfDisposableScheduler()
    {
        int called = 0;
        var (inner, disposableScheduler) = CreateScheduler();

        disposableScheduler.Schedule(1, inner.Now.AddHours(1), (i, action) => { called++; });
        called.Should().Be(0);
        inner.AdvanceBy(TimeSpan.FromHours(1).Ticks);

        called.Should().Be(1);
    }


    [Fact]
    public void PeriodicSchedulerShouldNotCallActionIfItIsDisposedDuringSchedule()
    {
        var (inner, disposableScheduler) = CreateScheduler();

        int called = 0;
        using var _ = disposableScheduler.SchedulePeriodic(TimeSpan.FromMinutes(1), () => called++);

        // Dispose before the time moves forward and trigger a schedule
        disposableScheduler.Dispose();

        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(0);
    }

    [Fact]
    public void PeriodicSchedulerShouldNotCallActionIfItIsDisposedBeforeScheduled()
    {
        var (inner, disposableScheduler) = CreateScheduler();

        int called = 0;
        // Dispose before the we call schedule
        disposableScheduler.Dispose();
        using var _ = disposableScheduler.SchedulePeriodic(TimeSpan.FromMinutes(1), () => called++);
        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(0);
    }

    [Fact]
    public void SchedulePeriodicStopsAfterDisposeOfDisposableScheduler()
    {
        var (inner, disposableScheduler) = CreateScheduler();

        int called = 0;
        using var _ = disposableScheduler.SchedulePeriodic(TimeSpan.FromMinutes(1), () => called++);

        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(1);

        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(2);

        disposableScheduler.Dispose();
        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(2);
    }

    [Fact]
    public void SchedulePeriodicStopsAfterDisposeOfSubscriber()
    {
        var (inner, disposableScheduler) = CreateScheduler();

        int called = 0;
        var sub = disposableScheduler.SchedulePeriodic(TimeSpan.FromMinutes(1), () => called++);

        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(1);

        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(2);

        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(3);

        sub.Dispose();

        inner.AdvanceBy(TimeSpan.FromMinutes(1).Ticks);
        called.Should().Be(3);
    }

    [Fact]
    public void DisposeTwice_NoException()
    {
        var (_, disposableScheduler) = CreateScheduler();

        disposableScheduler.Dispose();
        disposableScheduler.Dispose();
    }

    private static (TestScheduler inner, DisposableScheduler disposableScheduler) CreateScheduler()
    {
        var inner = new TestScheduler();
        inner.AdvanceTo(new DateTimeOffset(2022, 01, 12, 13, 8, 2, TimeSpan.FromHours(5)).Ticks);

        return (inner, new DisposableScheduler(inner));
    }
}
