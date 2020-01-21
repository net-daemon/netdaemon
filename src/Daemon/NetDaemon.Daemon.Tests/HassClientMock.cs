using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using Moq;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class HassClientMock : Mock<IHassClient>
    {
        internal ConcurrentQueue<HassEvent> FakeEvents = new ConcurrentQueue<HassEvent>();
        internal ConcurrentDictionary<string, HassState> FakeStates = new ConcurrentDictionary<string, HassState>();

        public HassClientMock()
        {
            // Setup common mocks
            Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);
            SetupGet(x => x.States).Returns(FakeStates);

            SetupDefaultStates();

            Setup(x => x.ReadEventAsync())
                .ReturnsAsync(() => { return FakeEvents.TryDequeue(out var ev) ? ev : null; });
        }

        public static HassClientMock DefaultMock => new HassClientMock();

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

        private void SetupDefaultStates()
        {
            FakeStates["light.correct_entity"] = new HassState
            {
                EntityId = "light.correct_entity",
                Attributes = new Dictionary<string, object>
                {
                    ["test"] = 100
                }
            };

            FakeStates["light.correct_entity2"] = new HassState
            {
                EntityId = "light.correct_entity2",
                Attributes = new Dictionary<string, object>
                {
                    ["test"] = 101
                }
            };

            FakeStates["switch.correct_entity"] = new HassState
            {
                EntityId = "switch.correct_entity",
                Attributes = new Dictionary<string, object>
                {
                    ["test"] = 105
                }
            };

            FakeStates["light.filtered_entity"] = new HassState
            {
                EntityId = "light.filtered_entity",
                Attributes = new Dictionary<string, object>
                {
                    ["test"] = 90
                }
            };
            FakeStates["binary_sensor.pir"] = new HassState
            {
                EntityId = "binary_sensor.pir",
                State = "off",
                Attributes = new Dictionary<string, object>
                {
                    ["device_class"] = "motion"
                }
            };
        }

        public void AddChangedEvent(string entityId, object fromState, object toState, DateTime lastUpdated, DateTime lastChanged)
        {
            // Todo: Refactor to smth smarter
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
                        State = toState,
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion"
                        }
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

        public void VerifyCallService(string domain, string service,
            params (string attribute, object value)[] attributesTuples)
        {
            var attributes = new ExpandoObject();
            foreach (var attributesTuple in attributesTuples)
                ((IDictionary<string, object>) attributes)[attributesTuple.attribute] = attributesTuple.value;

            Verify(n => n.CallService(domain, service, attributes), Times.AtLeastOnce);
        }

        public void VerifyCallServiceTimes(string service, Times times)
        {
            Verify(n => n.CallService(It.IsAny<string>(), service, It.IsAny<FluentExpandoObject>()), times);
        }

        public void AssertEqual(HassState hassState, EntityState entity)
        {
            Assert.Equal(hassState.EntityId, entity.EntityId);
            Assert.Equal(hassState.State, entity.State);
            Assert.Equal(hassState.LastChanged, entity.LastChanged);
            Assert.Equal(hassState.LastUpdated, entity.LastUpdated);

            foreach (var attribute in hassState.Attributes.Keys)
                Assert.Equal(hassState.Attributes[attribute],
                    ((IDictionary<string, object>) entity.Attribute)[attribute]);
        }

        /// <summary>
        /// Gets a cancellation source that does not timeout if debugger is attached
        /// </summary>
        /// <param name="milliSeconds"></param>
        /// <returns></returns>
        public CancellationTokenSource GetSourceWithTimeout(int milliSeconds)
        {
            return Debugger.IsAttached
                ? new CancellationTokenSource()
                : new CancellationTokenSource(10);
        }
    }
}