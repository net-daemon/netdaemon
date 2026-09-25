using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using Moq;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Extensions.Scheduler.SunEvents;
using Xunit;

namespace NetDaemon.Extensions.Scheduling.Tests;

public class SunEventTests
{
    [Fact]
    public void TestWhereSunEventMustStillHappenToday()
    {
        var count = 0;
        var sched = new TestScheduler();
        var mockSolarCalendar = new Mock<ISolarCalendar>();

        var beforeEvent = new DateTime(2026, 1, 1, 4, 0, 0);
        var eventTime = new DateTime(2026, 1, 1, 6, 0, 0);
        var endOfDay = new DateTime(2026, 1, 1, 23, 59, 0);

        var sunScheduler = new SunEventScheduler(mockSolarCalendar.Object, sched);
        sched.AdvanceTo(beforeEvent.ToUniversalTime().Ticks);
        var sub = sunScheduler.RunAtSunEvent(() => eventTime, () =>
        {
            count++;
        });

        count.Should().Be(0, because: "Sun event has not happened yet");
        sched.AdvanceTo(eventTime.ToUniversalTime().Ticks);
        count.Should().Be(1, because: "Sun event has now passed");
        sched.AdvanceTo(endOfDay.ToUniversalTime().Ticks);
        count.Should().Be(1, because: "Day has ended but sun event already happened earlier");
    }

    [Fact]
    public void TestWhereSunEventHasAlreadyHappenedToday()
    {
        var count = 0;
        var sched = new TestScheduler();
        var mockSolarCalendar = new Mock<ISolarCalendar>();

        var eventTime = new DateTime(2026, 1, 1, 6, 0, 0);
        var afterEvent = new DateTime(2026, 1, 1, 7, 0, 0);
        var endOfDay = new DateTime(2026, 1, 1, 23, 50, 0);

        var sunScheduler = new SunEventScheduler(mockSolarCalendar.Object, sched);
        sched.AdvanceTo(afterEvent.ToUniversalTime().Ticks);
        var sub = sunScheduler.RunAtSunEvent(() => eventTime, () =>
        {
            count++;
        });
        
        sched.AdvanceTo(endOfDay.ToUniversalTime().Ticks);
        count.Should().Be(0, because: "Sun event has already happened today");
    }

    [Fact]
    public void TestCheckingForEventOnNextDay()
    {
        var count = 0;
        var sched = new TestScheduler();
        var mockSolarCalendar = new Mock<ISolarCalendar>();

        var today = new DateTime(2026, 1, 1, 7, 0, 0);
        var beginningOfNextDay = new DateTime(2026, 1, 2, 0, 0, 0);
        var eventTime = new DateTime(2026, 1, 2, 6, 0, 0);

        var sunScheduler = new SunEventScheduler(mockSolarCalendar.Object, sched);
        sched.AdvanceTo(today.ToUniversalTime().Ticks);
        var sub = sunScheduler.RunAtSunEvent(() => eventTime, () =>
        {
            count++;
        });

        count.Should().Be(0, because: "Sun event has not happened yet");
        sched.AdvanceTo(beginningOfNextDay.ToUniversalTime().Ticks);
        count.Should().Be(0, because: "Event should be scheduled but not executed yet");
        sched.AdvanceTo(eventTime.ToUniversalTime().Ticks);
        count.Should().Be(1, because: "Event has now passed");
    }

    [Fact]
    public void TestSunriseGetsCorrectTime()
    {
        var count = 0;
        var sched = new TestScheduler();
        var mockSolarCalendar = new Mock<ISolarCalendar>();
        var eventTime = new DateTime(2026, 1, 1, 6, 0, 0);

        mockSolarCalendar.Setup(c => c.Sunrise).Returns(eventTime);
        var sunScheduler = new SunEventScheduler(mockSolarCalendar.Object, sched);

        var sub = sunScheduler.RunAtSunrise(() =>
        {
            count++;
        });
        mockSolarCalendar.VerifyAll();
    }

    [Fact]
    public void TestSunsetGetsCorrectTime()
    {
        var count = 0;
        var sched = new TestScheduler();
        var mockSolarCalendar = new Mock<ISolarCalendar>();
        var eventTime = new DateTime(2026, 1, 1, 6, 0, 0);

        mockSolarCalendar.Setup(c => c.Sunset).Returns(eventTime);
        var sunScheduler = new SunEventScheduler(mockSolarCalendar.Object, sched);

        var sub = sunScheduler.RunAtSunset(() =>
        {
            count++;
        });
        mockSolarCalendar.VerifyAll();
    }

    [Fact]
    public void TestDawnGetsCorrectTime()
    {
        var count = 0;
        var sched = new TestScheduler();
        var mockSolarCalendar = new Mock<ISolarCalendar>();
        var eventTime = new DateTime(2026, 1, 1, 6, 0, 0);

        mockSolarCalendar.Setup(c => c.Dawn).Returns(eventTime);
        var sunScheduler = new SunEventScheduler(mockSolarCalendar.Object, sched);

        var sub = sunScheduler.RunAtDawn(() =>
        {
            count++;
        });
        mockSolarCalendar.VerifyAll();
    }

    [Fact]
    public void TestDuskGetsCorrectTime()
    {
        var count = 0;
        var sched = new TestScheduler();
        var mockSolarCalendar = new Mock<ISolarCalendar>();
        var eventTime = new DateTime(2026, 1, 1, 6, 0, 0);

        mockSolarCalendar.Setup(c => c.Dusk).Returns(eventTime);
        var sunScheduler = new SunEventScheduler(mockSolarCalendar.Object, sched);

        var sub = sunScheduler.RunAtDusk(() =>
        {
            count++;
        });
        mockSolarCalendar.VerifyAll();
    }
}
