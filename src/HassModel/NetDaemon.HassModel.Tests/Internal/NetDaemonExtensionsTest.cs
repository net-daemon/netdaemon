using System.Reactive.Subjects;
using FluentAssertions.Extensions;
using Microsoft.Reactive.Testing;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.Internal;

public class NetDaemonExtensionsTest
{
    [Fact]
    public void TestThrottleAfterFirstEvent()
    {
        var subject = new Subject<int>();
        var scheduler = new TestScheduler();
        var startTime = new DateTime(2000, 2, 3).AsUtc();

        var delayed = subject.ThrottleAfterFirstEvent(TimeSpan.FromMinutes(10), scheduler);

        var results = new List<(int, int)>();
        delayed.Subscribe(e => results.Add((e, (int)(scheduler.Now - startTime).TotalMinutes)));

        // Test with two lists, first has the events to send at which time, second list has the events expected at which time
        var sendEventsAtTime =     new[] { (1, 0), (2,  1), (3, 15), (4, 16), (5, 36), (6, 38), (7, 98)};
        var expectedEventsAtTime = new[] { (1, 0), (2, 10),          (4, 20), (5, 36), (6, 46), (7, 98)};

        foreach (var (value, time) in sendEventsAtTime)
        {
            scheduler.AdvanceTo(startTime.AddMinutes(time).Ticks);
            subject.OnNext(value);
        }

        results.Should().BeEquivalentTo(expectedEventsAtTime);
    }
}
