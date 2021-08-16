using Microsoft.Extensions.Logging;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Newtonsoft.Json;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Mock of RxApp to test your own applications using
    ///     a separate implementations class
    /// </summary>
    public class RxAppMock : Mock<INetDaemonRxApp>
    {
        /// <summary>
        ///     Current entities and states
        /// </summary>
        public IList<EntityState> MockState { get; } = new List<EntityState>();

        /// <summary>
        ///     Observable fake states
        /// </summary>
        public ObservableBase<(EntityState Old, EntityState New)> StateChangesObservable { get; }

        /// <summary>
        ///     Observable fake events
        /// </summary>
        public ObservableBase<RxEvent> EventChangesObservable { get; }

        /// <summary>
        /// This is a Scheduler to support time travel for Observable Timer and Interval
        /// </summary>
        public TestScheduler TestScheduler { get; } = new();

        private readonly IDictionary<string, string> _mockDataRepository = new Dictionary<string, string>();

        /// <summary>
        ///     Default constructor
        /// </summary>
        public RxAppMock()
        {
            TestScheduler.AdvanceBy(TestScheduler.Now.AddYears(1).Ticks);
            var loggerMock = new Mock<ILogger>();
            StateChangesObservable = new StateChangeObservable(loggerMock.Object, Object);
            EventChangesObservable = new EventObservable(loggerMock.Object, Object);

            ReactiveEventMock r = new(this);//new EventObservable(loggerMock.Object, Object);

            Setup(n => n.EventChanges).Returns(r);
            Setup(n => n.Entity(It.IsAny<string>())).Returns<string>(entityId =>
            {
                var m = new Mock<IRxEntityBase>();
                m.Setup(n => n.StateChanges).Returns(StateChangesObservable.Where(f => f.New?.EntityId == entityId && f.New?.State != f.Old?.State));
                m.Setup(n => n.StateAllChanges).Returns(StateChangesObservable.Where(f => f.New?.EntityId == entityId));
                m.Setup(e => e.TurnOn(It.IsAny<object?>())).Callback<object?>(attributes => UpdateMockState(entityId, "on", attributes));
                m.Setup(e => e.TurnOff(It.IsAny<object?>())).Callback<object?>(attributes => UpdateMockState(entityId, "off", attributes));
                return m.Object;
            });

            Setup(n => n.State(It.IsAny<string>())).Returns<string>(entityId => MockState.First(state => state.EntityId == entityId));
            Setup(n => n.States).Returns(() => MockState);

            Setup(n => n.StateChanges).Returns(StateChangesObservable.Where(f => f.New.State != f.Old.State));
            Setup(n => n.StateAllChanges).Returns(StateChangesObservable);

            Setup(n => n.Entities(It.IsAny<string[]>())).Returns<string[]>(entityIds =>
            {
                var m = new Mock<IRxEntityBase>();
                m.Setup(n => n.StateChanges).Returns(StateChangesObservable.Where(f => entityIds.Contains(f.New.EntityId) && f.New?.State != f.Old?.State));
                m.Setup(n => n.StateAllChanges).Returns(StateChangesObservable.Where(f => entityIds.Contains(f.New.EntityId)));
                m.Setup(e => e.TurnOn(It.IsAny<object?>())).Callback<object?>(attributes => UpdateMockState(entityIds, "on", attributes));
                m.Setup(e => e.TurnOff(It.IsAny<object?>())).Callback<object?>(attributes => UpdateMockState(entityIds, "off", attributes));
                return m.Object;
            });

            Setup(n => n.Entities(It.IsAny<Func<IEntityProperties, bool>>())).Returns<Func<IEntityProperties, bool>>(func =>
            {
                var x = MockState.Where(func);
                var y = x.Select(n => n.EntityId).ToArray();
                var m = new Mock<IRxEntityBase>();
                m.Setup(n => n.StateChanges).Returns(StateChangesObservable.Where(f => y.Contains(f.New.EntityId) && f.New?.State != f.Old?.State));
                m.Setup(n => n.StateAllChanges).Returns(StateChangesObservable.Where(f => y.Contains(f.New.EntityId)));
                m.Setup(e => e.TurnOn(It.IsAny<object?>())).Throws(new NotImplementedException());
                m.Setup(e => e.TurnOff(It.IsAny<object?>())).Throws(new NotImplementedException());
                return m.Object;
            });
            
            Setup(s => s.SetState(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object?>(), It.IsAny<bool>()))
                    .Returns<string, object, object?, bool>((entityId, state, attributes, _) =>
                    {
                        var entityState = new EntityState
                        {
                            EntityId = entityId,
                            State = state,
                            Attribute = attributes
                        };

                        var existingState = MockState.FirstOrDefault(e => e.EntityId == entityId);
                        if (existingState is not null)
                        {
                            MockState.Remove(existingState);
                        }
                        
                        MockState.Add(entityState);

                        return entityState;
                    });
            
            // Scheduler Setups
            Setup(s => s.RunIn(It.IsAny<TimeSpan>(), It.IsAny<Action>())).Returns<TimeSpan, Action>((span, action) =>
            {
                return Observable.Timer(span, TestScheduler)
                    .Subscribe(_ => action());
            });

            Setup(s => s.RunEveryMinute(It.IsAny<short>(), It.IsAny<Action>())).Returns<short, Action>((second, action) =>
            {
                var now = TestScheduler.Now;
                var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute + 1, second);
                return Observable.Timer(startTime, TimeSpan.FromMinutes(1), TestScheduler)
                    .Subscribe(_ => action());
            });

            Setup(s => s.RunEveryHour(It.IsAny<string>(), It.IsAny<Action>())).Returns<string, Action>((time, action) =>
            {
                time = $"{TestScheduler.Now.Hour:D2}:{time}";

                if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                {
                    throw new FormatException($"{time} is not a valid time for the current locale");
                }

                var now = TestScheduler.Now;
                var timeOfDayToTrigger = new DateTime(
                    now.Year,
                    now.Month,
                    now.Day,
                    now.Hour,
                    parsedTime.Minute,
                    parsedTime.Second
                );

                if (now > timeOfDayToTrigger)
                {
                    // It is not due until the next hour
                    timeOfDayToTrigger = timeOfDayToTrigger.AddHours(1);
                }

                return Observable.Timer(timeOfDayToTrigger, TimeSpan.FromHours(1), TestScheduler)
                    .Subscribe(_ => action());
            });

            Setup(s => s.RunDaily(It.IsAny<string>(), It.IsAny<Action>())).Returns<string, Action>((time, action) =>
            {
                if (!DateTime.TryParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                {
                    throw new FormatException($"{time} is not a valid time for the current locale");
                }

                var now = TestScheduler.Now;
                var timeOfDayToTrigger = new DateTime(
                    now.Year,
                    now.Month,
                    now.Day,
                    now.Hour,
                    parsedTime.Minute,
                    parsedTime.Second
                );

                if (now > timeOfDayToTrigger)
                {
                    // It is not due until the next hour
                    timeOfDayToTrigger = timeOfDayToTrigger.AddDays(1);
                }

                return Observable.Timer(timeOfDayToTrigger, TimeSpan.FromDays(1), TestScheduler)
                    .Subscribe(_ => action());
            });

            Setup(s => s.RunIn(It.IsAny<TimeSpan>(), It.IsAny<Action>()))
                .Callback<TimeSpan, Action>((span, action) =>
                {
                    Observable.Timer(span, TestScheduler)
                        .Subscribe(_ => action());
                });

            Setup(s => s.SaveData(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback<string, object>((id, data) =>
                    {
                        if (_mockDataRepository.ContainsKey(id))
                        {
                            _mockDataRepository.Remove(id);
                        }

                        _mockDataRepository.Add(id, JsonConvert.SerializeObject(data));
                    });

            Setup(s => s.GetData<object>(It.IsAny<string>()))
                    .Returns(new InvocationFunc(invocation =>
                    {
                        var id = (string)invocation.Arguments[0];
                        var returnType = invocation.Method.GetGenericArguments()[0];

                        return _mockDataRepository.TryGetValue(id, out var value) ? JsonConvert.DeserializeObject(value, returnType) : null;
                    }));
        }

        private void UpdateMockState(string[] entityIds, string newState, object? attributes)
        {
            foreach (var entityId in entityIds) UpdateMockState(entityId, newState, attributes);
        }

        private void UpdateMockState(string entityId, string newState, object? attributes)
        {
            var state = MockState.FirstOrDefault(e => e.EntityId == entityId);
            if (state == null) return;
            MockState.Remove(state);
            MockState.Add(new EntityState() { EntityId = entityId, State = newState, Attribute = attributes });
        }
        
        /// <summary>
        ///     Triggers an general Home Assistant event
        /// </summary>
        /// <param name="haEvent">Event to trigger</param>
        public void TriggerEvent(RxEvent haEvent)
        {
            foreach (var observer in ((EventObservable)EventChangesObservable).Observers)
            {
                try
                {
                    observer.OnNext(haEvent);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                    throw;
                }
            }
        }

        /// <summary>
        ///     Triggers an general Home Assistant event
        /// </summary>
        /// <param name="haEvent">Event name</param>
        /// <param name="domain">Domain</param>
        /// <param name="data">Data provided by event</param>
        public void TriggerEvent(string haEvent, string? domain, dynamic? data)
        {
            foreach (var observer in ((EventObservable)EventChangesObservable).Observers)
            {
                try
                {
                    observer.OnNext(new RxEvent(haEvent, domain, data));
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                    throw;
                }
            }
        }

        /// <summary>
        ///     Trigger state change
        /// </summary>
        /// <param name="oldState">Old state</param>
        /// <param name="newState">New state</param>
        public void TriggerStateChange(EntityState oldState, EntityState newState)
        {
            var state = MockState.FirstOrDefault(entity => entity.EntityId == newState.EntityId);
            var index = MockState.IndexOf(state!);

            if (index != -1)
                MockState[index] = newState;

            // Call the observable with no blocking
            foreach (var observer in ((StateChangeObservable)StateChangesObservable).Observers)
            {
                try
                {
                    observer.OnNext((oldState, newState));
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                    throw;
                }
            }
        }

        /// <summary>
        ///     Trigger event
        /// </summary>
        /// <param name="entityId">Unique id of the entity</param>
        /// <param name="oldState">Old state</param>
        /// <param name="newState">New state</param>
        public void TriggerStateChange(string entityId, object? oldState, object? newState)
        {
            TriggerStateChange(
                                new EntityState
                                {
                                    EntityId = entityId,
                                    State = oldState
                                },
                              new EntityState
                              {
                                  EntityId = entityId,
                                  State = newState
                              }

                            );
        }

        /// <summary>
        ///     Trigger event
        /// </summary>
        /// <param name="entityId">Unique id of the entity</param>
        /// <param name="oldState">Old state</param>
        /// <param name="oldAttributes">Old attributes</param>
        /// <param name="newState">New state</param>
        /// <param name="newAttributes">New attributes</param>
        public void TriggerStateChange(string entityId, object? oldState, dynamic? oldAttributes, object? newState, dynamic? newAttributes)
        {
            TriggerStateChange(
                                new EntityState
                                {
                                    EntityId = entityId,
                                    State = oldState,
                                    Attribute = oldAttributes
                                },
                                new EntityState
                                {
                                    EntityId = entityId,
                                    State = newState,
                                    Attribute = newAttributes
                                }
                            );
        }

        /// <summary>
        ///     Instance new dynamic attribute providing name-value pairs
        /// </summary>
        /// <param name="attr">Name value pair tuple of attribute name and attribute value</param>
        public static dynamic NewAttribute(params (string, object)[] attr)
        {
            var returnValue = new FluentExpandoObject();
            foreach (var (name, value) in attr)
            {
                returnValue[name] = value;
            }
            return returnValue;
        }

        /// <summary>
        ///     Verify CallService been called using Moq.Times.
        /// </summary>
        /// <param name="domain">Domain of service call</param>
        /// <param name="service">Service to bee called</param>
        /// <param name="data">Data sent by service</param>
        /// <param name="times">Times checking</param>
        public void VerifyCallService(string? domain = null, string? service = null, dynamic? data = null, Times? times = null)
        {
            var t = times ?? Times.Once();
            domain ??= It.IsAny<string>();
            service ??= It.IsAny<string>();
            data ??= It.IsAny<object>();
            Verify(x => x.CallService(domain, service, It.IsAny<object>(), It.IsAny<bool>()), t);
        }

        /// <summary>
        ///     Verifies that the Entity turned on
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="attributes"></param>
        /// <param name="times">Nr of times called</param>
        public void VerifyEntityTurnOn(string entityId, dynamic? attributes = null, Times? times = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            var t = times ?? Times.Once();

            if (attributes is null)
            {
                Verify(x => x.Entity(entityId).TurnOn(It.IsAny<object>()), t);
            }
            else
            {
                Verify(x => x.Entity(entityId).TurnOn((object)attributes), t);
            }
        }

        /// <summary>
        ///     Verifies that the Entity turned off
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="attributes"></param>
        /// <param name="times">Nr of times called</param>
        public void VerifyEntityTurnOff(string entityId, dynamic? attributes = null, Times? times = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");
            var t = times ?? Times.Once();
            if (attributes is null)
            {
                Verify(x => x.Entity(entityId).TurnOff(It.IsAny<object>()), t);
            }
            else
            {
                Verify(x => x.Entity(entityId).TurnOff((object)attributes), t);
            }
        }

        /// <summary>
        ///     Verifies that the Entity toggles
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="attributes"></param>
        /// <param name="times">Nr of times called</param>
        public void VerifyEntityToggle(string entityId, dynamic? attributes = null, Times? times = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            var t = times ?? Times.Once();

            if (attributes is null)
            {
                Verify(x => x.Entity(entityId).Toggle(It.IsAny<object>()), t);
            }
            else
            {
                Verify(x => x.Entity(entityId).Toggle((object)attributes), t);
            }
        }

        /// <summary>
        ///     Verifies that the Entity set state
        /// </summary>
        /// <param name="entityId">Unique id of entity</param>
        /// <param name="state">State to set</param>
        /// <param name="attributes">Attributes provided</param>
        /// <param name="times">Nr of times called</param>
        public void VerifyEntitySetState(string entityId, dynamic? state = null
        , dynamic? attributes = null, Times? times = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            if (state is not null && state is not object)
                throw new NotSupportedException("state needs to be an object");
            var t = times ?? Times.Once();
            if (state is not null)
            {
                if (attributes is not null)
                {
                    Verify(x => x.Entity(entityId).SetState(
                        (object)state,
                        (object)attributes,
                        It.IsAny<bool>()),
                        t);
                }
                else
                {
                    Verify(x => x.Entity(entityId).SetState(
                        (object)state,
                        It.IsAny<object>(),
                        It.IsAny<bool>()),
                        t);
                }
            }
            else
            {
                if (attributes is not null)
                {
                    Verify(x => x.Entity(entityId).SetState(
                        It.IsAny<object>(),
                        (object)attributes,
                        It.IsAny<bool>()),
                        t);
                }
                else
                {
                    Verify(x => x.Entity(entityId).SetState(
                        It.IsAny<object>(),
                        It.IsAny<object>(),
                        It.IsAny<bool>()),
                        t);
                }
            }
        }

        /// <summary>
        ///     Verify state
        /// </summary>
        /// <param name="entityId">Unique id</param>
        /// <param name="state">State to verify</param>
        /// <param name="attributes">Attributes to verify</param>
        public void VerifyState(string entityId, dynamic? state = null, dynamic? attributes = null)
        {
            var stateResult = false;
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            if (state is not null && state is not object)
                throw new NotSupportedException("state needs to be an object");

            var mockState = MockState.First(e => e.EntityId == entityId);

            if (state is not null)
            {
                if (attributes is not null)
                    stateResult = mockState.State == state && mockState.Attribute == attributes;
                else
                    stateResult = mockState.State == state;
            }

            if (attributes is not null)
                stateResult = mockState.Attribute == attributes;

            if (!stateResult) throw new ArgumentOutOfRangeException(nameof(state), "State does not match");
        }

        /// <summary>
        ///     Verifies SetState been called
        /// </summary>
        /// <param name="entityId">Unique id of entity</param>
        /// <param name="state">State to set</param>
        /// <param name="attributes">Attributes provided</param>
        /// <param name="times">Times called</param>
        public void VerifySetState(string entityId, dynamic? state = null, dynamic? attributes = null, Times? times = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            if (state is not null && state is not object)
                throw new NotSupportedException("state needs to be an object");

            var t = times ?? Times.Once();

            if (state is not null)
            {
                if (attributes is not null)
                {
                    Verify(x => x.SetState(
                        entityId,
                        (object)state,
                        (object)attributes, false),
                        t);
                }
                else
                {
                    Verify(x => x.SetState(
                        entityId,
                        (object)state,
                        It.IsAny<object>(), false),
                        t);
                }
            }
            else
            {
                if (attributes is not null)
                {
                    Verify(x => x.SetState(
                        entityId,
                        It.IsAny<object>(),
                        (object)attributes, false),
                        t);
                }
                else
                {
                    Verify(x => x.SetState(
                        entityId,
                        It.IsAny<object>(),
                        It.IsAny<object>(), false),
                        t);
                }
            }
        }
    }

    /// <summary>
    ///     Implements Observable RxEvent
    /// </summary>
    public class ReactiveEventMock : IRxEvent
    {
        private readonly RxAppMock _daemonRxApp;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        public ReactiveEventMock(RxAppMock daemon)
        {
            _daemonRxApp = daemon;
        }

        /// <summary>
        ///     Implements IObservable ReactiveEvent
        /// </summary>
        /// <param name="observer">Observer</param>
        public IDisposable Subscribe(IObserver<RxEvent> observer)
        {
            return _daemonRxApp!.EventChangesObservable.Subscribe(observer);
        }
    }
}