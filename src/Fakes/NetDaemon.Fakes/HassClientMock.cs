using JoySoftware.HomeAssistant.Client;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Common;
using Xunit;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Mock for HassClient
    /// </summary>
    public class HassClientMock : Mock<IHassClient>
    {
        /// <summary>
        ///     Default HassClientFactory mock 
        /// </summary>
        public HassClientFactoryMock DefaultHassClientFactoryMock { get; }
        /// <summary>
        ///     Fake areas in HassClient
        /// </summary>
        public HassAreas Areas { get; } = new();

        /// <summary>
        ///     Fake devices in HassClient
        /// </summary>
        /// <returns></returns>
        public HassDevices Devices { get; } = new();

        /// <summary>
        ///     Fake entities in HassClient
        /// </summary>
        public HassEntities Entities { get; } = new();

        /// <summary>
        ///     Fake events in HassClient
        /// </summary>
        public ConcurrentQueue<HassEvent> FakeEvents { get; } = new();
        /// <summary>
        ///     All current fake entities and it's states
        /// </summary>
        public ConcurrentDictionary<string, HassState> FakeStates { get; } = new();

        /// <summary>
        ///     Default constructor
        /// </summary>
        public HassClientMock()
        {
#pragma warning disable 8619, 8620
            // Setup common mocks
            Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            SetupGet(x => x.States).Returns(FakeStates);

            Setup(x => x.GetAllStates(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => FakeStates.Values);

            Setup(x => x.ReadEventAsync())
                .ReturnsAsync(() => FakeEvents.TryDequeue(out var ev) ? ev : null);

            Setup(x => x.ReadEventAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => FakeEvents.TryDequeue(out var ev) ? ev : null);

            Setup(x => x.GetConfig()).ReturnsAsync(new HassConfig { State = "RUNNING" });

            Setup(x => x.SetState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>())).Returns<string, string, object>(
                (entityId, state, attributes) =>
                {
                    var fluentAttr = attributes.ToExpandoObject();
                    if (fluentAttr is not null)
                    {
                        var attrib = new Dictionary<string, object>();
                        foreach (var attr in (IDictionary<string, object>)fluentAttr)
                            attrib[attr.Key] = attr.Value;
                        return Task.FromResult(new HassState
                        {
                            EntityId = entityId,
                            State = state,
                            Attributes = attrib
                        });
                    }
                    return Task.FromResult(new HassState
                    {
                        EntityId = entityId,
                        State = state,
                        Attributes = null
                    });
                }
            );

            Setup(n => n.GetAreas()).ReturnsAsync(Areas);
            Setup(n => n.GetDevices()).ReturnsAsync(Devices);
            Setup(n => n.GetEntities()).ReturnsAsync(Entities);

            // Setup one with area
            Devices.Add(new HassDevice { Id = "device_idd", AreaId = "area_idd" });
            Areas.Add(new HassArea { Name = "Area", Id = "area_idd" });
            Entities.Add(new HassEntity
            {
                EntityId = "light.light_in_area",
                DeviceId = "device_idd"
            });
            DefaultHassClientFactoryMock = new HassClientFactoryMock(this);
        }

        /// <summary>
        ///     Default instance of mock
        /// </summary>
        public static HassClientMock DefaultMock => new();

        /// <summary>
        ///     Returns a mock that will always return false when connect to Home Assistant
        /// </summary>
        public static HassClientMock MockConnectFalse
        {
            get
            {
                var mock = DefaultMock;
                mock.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<bool>(),
                        It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(false);
                return mock;
            }
        }

        /// <summary>
        ///     Adds a new call_service event to mock
        /// </summary>
        /// <param name="domain">Domain of service</param>
        /// <param name="service">Service to fake</param>
        /// <param name="data">Data sent by service</param>
        public void AddCallServiceEvent(string domain, string service, dynamic? data = null)
        {
            // Todo: Refactor to something smarter
            FakeEvents.Enqueue(new HassEvent
            {
                EventType = "call_service",
                Data = new HassServiceEventData
                {
                    Domain = domain,
                    Service = service,
                    Data = data
                }
            });
        }

        /// <summary>
        ///     Adds a new changed event
        /// </summary>
        /// <param name="entityId">Id of entity that has changes in state</param>
        /// <param name="fromState">Old state</param>
        /// <param name="toState">New state</param>
        /// <param name="lastUpdated">Last updated</param>
        /// <param name="lastChanged">Last changed</param>
        public void AddChangedEvent(string entityId, object fromState, object toState, DateTime lastUpdated, DateTime lastChanged)
        {
            // Todo: Refactor to something smarter
            FakeEvents.Enqueue(new HassEvent
            {
                EventType = "state_changed",
                Data = new HassStateChangedEventData
                {
                    EntityId = entityId,
                    NewState = new HassState
                    {
                        State = toState,
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion"
                        },
                        LastChanged = lastChanged,
                        LastUpdated = lastUpdated
                    },
                    OldState = new HassState
                    {
                        State = fromState,
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion"
                        }
                    }
                }
            });
        }

        /// <summary>
        ///     Adds a changed event for entity
        /// </summary>
        /// <param name="entityId">Id of entity</param>
        /// <param name="fromState">From state</param>
        /// <param name="toState">To state</param>
        public void AddChangedEvent(string entityId, object fromState, object toState)
        {
            FakeEvents.Enqueue(new HassEvent
            {
                EventType = "state_changed",
                Data = new HassStateChangedEventData
                {
                    EntityId = entityId,
                    NewState = new HassState
                    {
                        EntityId = entityId,
                        State = toState,
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion"
                        },
                        LastUpdated = DateTime.Now,
                        LastChanged = DateTime.Now
                    },
                    OldState = new HassState
                    {
                        EntityId = entityId,
                        State = fromState,
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion"
                        }
                    }
                },
            });
        }

        /// <summary>
        ///     Adds a changed event for entity
        /// </summary>
        /// <param name="fromState">The from state advanced details</param>
        /// <param name="toState">The to state advanced details</param>
        public void AddChangeEventFull(HassState? fromState, HassState? toState)
        {
            FakeEvents.Enqueue(new HassEvent
            {
                EventType = "state_changed",
                Data = new HassStateChangedEventData
                {
                    EntityId = toState?.EntityId ?? fromState?.EntityId ?? "",
                    NewState = toState,
                    OldState = fromState
                },
            });
        }

        /// <summary>
        ///     Adds a custom event
        /// </summary>
        /// <param name="eventType">Type of event</param>
        /// <param name="data">Data sent by event</param>
        public void AddCustomEvent(string eventType, dynamic? data)
        {
            FakeEvents.Enqueue(new HassEvent
            {
                EventType = eventType,
                Data = data
            });
        }

        /// <summary>
        ///     Assert if the HassClient entity state is equals to NetDaemon entity state
        /// </summary>
        /// <param name="hassState">HassClient state instance</param>
        /// <param name="entity">NetDaemon state instance</param>
        public static void AssertEqual(HassState hassState, EntityState entity)
        {
            _ = hassState ??
               throw new NetDaemonArgumentNullException(nameof(hassState));
            _ = entity ??
               throw new NetDaemonArgumentNullException(nameof(entity));
            Assert.Equal(hassState.EntityId, entity.EntityId);
            Assert.Equal(hassState.State, entity.State);
            Assert.Equal(hassState.LastChanged, entity.LastChanged);
            Assert.Equal(hassState.LastUpdated, entity.LastUpdated);

            if (hassState.Attributes?.Keys == null || entity.Attribute == null)
                return;

            foreach (var attribute in hassState.Attributes!.Keys)
            {
                var attr = entity.Attribute as IDictionary<string, object> ??
                    throw new NetDaemonNullReferenceException($"{nameof(entity.Attribute)} cant be null");

                Assert.True(attr.ContainsKey(attribute));
                Assert.Equal(hassState.Attributes[attribute],
                    attr![attribute]);
            }
        }

        /// <summary>
        /// Gets a cancellation source that does not timeout if debugger is attached
        /// </summary>
        /// <param name="milliSeconds"></param>
        public static CancellationTokenSource GetSourceWithTimeout(int milliSeconds = 100)
        {
            return Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
        }

        /// <summary>
        ///     Verifies that call_service is called
        /// </summary>
        /// <param name="domain">Service domain</param>
        /// <param name="service">Service to verify</param>
        /// <param name="attributesTuples">Attributes sent by service</param>
        public void VerifyCallServiceTuple(string domain, string service,
            params (string attribute, object value)[] attributesTuples)
        {
            var attributes = new FluentExpandoObject();
            foreach (var (attribute, value) in attributesTuples)
                attributes[attribute] = value;

            Verify(n => n.CallService(domain, service, attributes, It.IsAny<bool>()), Times.AtLeastOnce);
        }

        /// <summary>
        ///     Verifies that call_service is called
        /// </summary>
        /// <param name="domain">Service domain</param>
        /// <param name="service">Service to verify</param>
        /// <param name="data">Data sent by service</param>
        /// <param name="waitForResponse">If service was waiting for response</param>
        /// <param name="times">Number of times called</param>
        public void VerifyCallService(string domain, string service, object? data = null, bool waitForResponse = false, Times? times = null)
        {
            if (times is not null)
                Verify(n => n.CallService(domain, service, data!, waitForResponse), times.Value);
            else
                Verify(n => n.CallService(domain, service, data!, waitForResponse), Times.AtLeastOnce);
        }

        /// <summary>
        ///     Verifies that call_service is called
        /// </summary>
        /// <param name="domain">Service domain</param>
        /// <param name="service">Service to verify</param>
        /// <param name="waitForResponse">If service was waiting for response</param>
        /// <param name="times">Number of times called</param>
        public void VerifyCallService(string domain, string service, bool waitForResponse = false, Times? times = null)
        {
            if (times is not null)
                Verify(n => n.CallService(domain, service, It.IsAny<object>(), waitForResponse), times.Value);
            else
                Verify(n => n.CallService(domain, service, It.IsAny<object>(), waitForResponse), Times.AtLeastOnce);
        }

        /// <summary>
        ///     Verifies service being sent "times" times
        /// </summary>
        /// <param name="service">The service to check</param>
        /// <param name="times">The number of times it should been called</param>
        public void VerifyCallServiceTimes(string service, Times times)
        {
            Verify(n => n.CallService(It.IsAny<string>(), service, It.IsAny<FluentExpandoObject>(), It.IsAny<bool>()), times);
        }

        /// <summary>
        ///     Verify state if entity
        /// </summary>
        /// <param name="entity">Entity to verify</param>
        /// <param name="state">The state to verify</param>
        /// <param name="attributesTuples">Attributes to verify</param>
        public void VerifySetState(string entity, string state,
            params (string attribute, object value)[] attributesTuples)
        {
            var attributes = new FluentExpandoObject();
            foreach (var (attribute, value) in attributesTuples)
                attributes[attribute] = value;

            Verify(n => n.SetState(entity, state, attributes), Times.AtLeastOnce);
        }

        /// <summary>
        ///     Verify that the state been set number of times
        /// </summary>
        /// <param name="entity">Entity that the state being set</param>
        /// <param name="times">Number of times set</param>
        public void VerifySetStateTimes(string entity, Times times)
        {
            Verify(n => n.SetState(entity, It.IsAny<string>(), It.IsAny<FluentExpandoObject>()), times);
        }
    }
}