using Microsoft.Extensions.Logging;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Fluent;
using NetDaemon.Common.Reactive;
using System;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Mock of RxApp to test your own applications using
    ///     a separate implementations class
    /// </summary>
    public class RxAppMock : Mock<INetDaemonRxApp>
    {
        /// <summary>
        ///     Observable fake states
        /// </summary>
        public ObservableBase<(EntityState, EntityState)> StateChangesObservable { get; }

        /// <summary>
        ///     Observalbe fake events
        /// </summary>
        /// <value></value>
        public ObservableBase<RxEvent> EventChangesObservable { get; }

        /// <summary>
        ///     Default constructor
        /// </summary>
        public RxAppMock()
        {
            var loggerMock = new Mock<ILogger>();
            StateChangesObservable = new StateChangeObservable(loggerMock.Object, Object);
            EventChangesObservable = new EventObservable(loggerMock.Object, Object);

            ReactiveEventMock r = new(this);//new EventObservable(loggerMock.Object, Object);
            var m = new Mock<IRxEntityBase>();
            m.Setup(n => n.StateChanges).Returns(StateChangesObservable);
            m.Setup(n => n.StateAllChanges).Returns(StateChangesObservable);
            Setup(n => n.EventChanges).Returns(r);
            Setup(n => n.Entity(It.IsAny<string>())).Returns<string>(_ => m.Object);
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
        /// <param name="times">Times checking</param>
        /// <param name="domain">Domain of service call</param>
        /// <param name="service">Service to bee called</param>
        /// <param name="data">Data sent by service</param>
        public void VerifyCallService(Times times, string? domain = null, string? service = null, dynamic? data = null)
        {
            domain ??= It.IsAny<string>();
            service ??= It.IsAny<string>();
            data ??= It.IsAny<object>();
            Verify(x => x.CallService(domain, service, It.IsAny<object>()), times);
        }

        /// <summary>
        ///     Verifies that the Entity turned on
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="attributes"></param>
        public void VerifyEntityTurnOn(string entityId, dynamic? attributes = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            if (attributes is null)
            {
                Verify(x => x.Entity(entityId).TurnOn(It.IsAny<object>()), Times.Once);
            }
            else
            {
                Verify(x => x.Entity(entityId).TurnOn((object)attributes), Times.Once);
            }
        }

        /// <summary>
        ///     Verifies that the Entity turned off
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="attributes"></param>
        public void VerifyEntityTurnOff(string entityId, dynamic? attributes = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            if (attributes is null)
            {
                Verify(x => x.Entity(entityId).TurnOff(It.IsAny<object>()), Times.Once);
            }
            else
            {
                Verify(x => x.Entity(entityId).TurnOff((object)attributes), Times.Once);
            }
        }

        /// <summary>
        ///     Verifies that the Entity toggles
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="attributes"></param>
        public void VerifyEntityToggle(string entityId, dynamic? attributes = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            if (attributes is null)
            {
                Verify(x => x.Entity(entityId).Toggle(It.IsAny<object>()), Times.Once);
            }
            else
            {
                Verify(x => x.Entity(entityId).Toggle((object)attributes), Times.Once);
            }
        }

        /// <summary>
        ///     Verifies that the Entity set state
        /// </summary>
        /// <param name="entityId">Unique id of entity</param>
        /// <param name="state">State to set</param>
        /// <param name="attributes">Attributes provided</param>
        public void VerifyEntitySetState(string entityId, dynamic? state, dynamic? attributes = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            if (state is not null && state is not object)
                throw new NotSupportedException("state needs to be an object");

            if (state is not null)
            {
                if (attributes is not null)
                {
                    Verify(x => x.Entity(entityId).SetState(
                        (object)state,
                        (object)attributes),
                        Times.Once);
                }
                else
                {
                    Verify(x => x.Entity(entityId).SetState(
                        (object)state,
                        It.IsAny<object>()),
                        Times.Once);
                }
            }
            else
            {
                if (attributes is not null)
                {
                    Verify(x => x.Entity(entityId).SetState(
                        It.IsAny<object>(),
                        (object)attributes),
                        Times.Once);
                }
                else
                {
                    Verify(x => x.Entity(entityId).SetState(
                        It.IsAny<object>(),
                        It.IsAny<object>()),
                        Times.Once);
                }
            }
        }

        /// <summary>
        ///     Verifies SetState been called
        /// </summary>
        /// <param name="entityId">Unique id of entity</param>
        /// <param name="state">State to set</param>
        /// <param name="attributes">Attributes provided</param>
        public void VerifySetState(string entityId, dynamic? state, dynamic? attributes = null)
        {
            if (attributes is not null && attributes is not object)
                throw new NotSupportedException("attributes needs to be an object");

            if (state is not null && state is not object)
                throw new NotSupportedException("state needs to be an object");

            if (state is not null)
            {
                if (attributes is not null)
                {
                    Verify(x => x.SetState(
                        entityId,
                        (object)state,
                        (object)attributes),
                        Times.Once);
                }
                else
                {
                    Verify(x => x.SetState(
                        entityId,
                        (object)state,
                        It.IsAny<object>()),
                        Times.Once);
                }
            }
            else
            {
                if (attributes is not null)
                {
                    Verify(x => x.SetState(
                        entityId,
                        It.IsAny<object>(),
                        (object)attributes),
                        Times.Once);
                }
                else
                {
                    Verify(x => x.SetState(
                        entityId,
                        It.IsAny<object>(),
                        It.IsAny<object>()),
                        Times.Once);
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