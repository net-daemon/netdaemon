using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Tests.TestHelpers;

namespace NetDaemon.HassModel.Tests;

public class StateObservableExtensionsTest
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
}