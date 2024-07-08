using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Tests.TestHelpers;

namespace NetDaemon.HassModel.Tests;

public sealed class StateObservableExtensionsTest : IDisposable
{
    private readonly Subject<StateChange> _subject = new();

    private readonly TestScheduler _testScheduler = new();
    private readonly IObservable<NumericStateChange> _numericStateChangeObservable;

    public StateObservableExtensionsTest()
    {
        _numericStateChangeObservable = _subject.Select(e => new NumericStateChange(new NumericEntity(e.Entity),
                                                                    EntityState.Map<NumericEntityState>(e.Old),
                                                                    EntityState.Map<NumericEntityState>(e.New)));
    }

    [Fact]
    public void TestThrottleShouldNotCallActionUsingIngoreOnCompleteWhenSubjectCompleted()
    {
        bool isCalled = false;

        _subject.Where(n => n?.New?.State == "off").IgnoreOnComplete().Throttle(TimeSpan.FromSeconds(1), _testScheduler).Subscribe(_ => { isCalled = true;});

        _subject.OnNext(new StateChange(new Entity(new Mock<IHaContext>().Object, ""), new EntityState { State = "on" }, new EntityState { State = "off" }));

        _subject.OnCompleted();

        isCalled.Should().BeFalse();
    }

    [Fact]
    public void TestThrottleShouldCallActionUsingIngoreOnCompleteInNormalOperation()
    {
        bool isCalled = false;

        _subject.Where(n => n?.New?.State == "off").IgnoreOnComplete().Throttle(TimeSpan.FromSeconds(1), _testScheduler).Subscribe(_ => { isCalled = true;});

        _subject.OnNext(new StateChange(new Entity(new Mock<IHaContext>().Object, ""), new EntityState { State = "on" }, new EntityState { State = "off" }));

        _testScheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks);
        _subject.OnCompleted();

        isCalled.Should().BeTrue();
    }

    [Fact]
    public void TestThatWhenStateIsForDoesNotCallActionWhenCompleted()
    {
        bool isCalled = false;

        _subject.WhenStateIsFor(n => n?.State == "off", TimeSpan.FromSeconds(10), _testScheduler).Subscribe(_ => { isCalled = true;});

        _subject.OnNext(new StateChange(new Entity(new Mock<IHaContext>().Object, ""), new EntityState { State = "on" }, new EntityState { State = "off" }));

        _subject.OnCompleted();

        isCalled.Should().BeFalse();
    }

    [Fact]
    public void TestNumericEntityWhenStateIsForDoesNotCallActionWhenCompleted()
    {
        bool isCalled = false;

        _numericStateChangeObservable.WhenStateIsFor(n => n?.State > 20, TimeSpan.FromSeconds(10), _testScheduler).Subscribe(_ => { isCalled = true;});

        _subject.OnNext(new StateChange(new Entity(new Mock<IHaContext>().Object, ""), new EntityState { State = "1" }, new EntityState { State = "30" }));

        _subject.OnCompleted();

        isCalled.Should().BeFalse();
    }

    [Fact]
    public void WhenNumStateIsForFiresInTime()
    {
        // wait for the sensor to be "on" for at least 10 ticks
        var eventTimes = _subject
            .WhenStateIsFor(e => e.IsOn(), TimeSpan.FromTicks(10), _testScheduler)
            .Select(_ => _testScheduler.Now.Ticks).SubscribeMock();

        TriggerStateChange(tick: 10, "on", "off"); // this should not start the timer
        TriggerStateChange(tick: 30, "off", "on"); // this does start the timer
        TriggerStateChange(tick: 35, "on", "off"); // this stops the timer before it reaches 10
        TriggerStateChange(tick: 40, "off", "on"); // this starts the timer again
        TriggerStateChange(tick: 45, "on", "on");  // this event should not stop the timer

        _testScheduler.AdvanceBy(100);

        eventTimes.Verify(m => m.OnNext(50), Times.Once); // event should be fired at 10 ticks after 40
        eventTimes.VerifyNoOtherCalls();
    }

    [Fact]
    public void WhenNumericStateIsForFiresInTime()
    {
        // wait for the sensor to be > 20 for at least 10 ticks
        var eventTimes = _numericStateChangeObservable.WhenStateIsFor(e => e?.State > 20, TimeSpan.FromTicks(10), _testScheduler)
            .Select(_ => _testScheduler.Now.Ticks).SubscribeMock();

        TriggerStateChange(tick: 10, "15", "19"); // this should not start the timer
        TriggerStateChange(tick: 30, "19", "21"); // this does start the timer
        TriggerStateChange(tick: 35, "21", "15"); // this stops the timer before it reaches 10
        TriggerStateChange(tick: 40, "15", "22"); // this starts the timer again
        TriggerStateChange(tick: 45, "22", "25"); // this event should not stop the timer

        _testScheduler.AdvanceTo(100);

        eventTimes.Verify(m => m.OnNext(50), Times.Once); // event should be fired at 10 ticks after 40
        eventTimes.VerifyNoOtherCalls();

    }

    private void TriggerStateChange(long tick, string old, string @new)
    {
        _testScheduler.AdvanceTo(tick);
        _subject.OnNext(new StateChange(new Entity(Mock.Of<IHaContext>(), "Dummy"), new EntityState { State = old }, new EntityState { State = @new }));
    }

    public void Dispose()
    {
       _subject.Dispose();
    }
}
