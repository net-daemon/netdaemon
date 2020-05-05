using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class HassClientMock : Mock<IHassClient>
    {
        internal ConcurrentQueue<HassEvent> FakeEvents = new ConcurrentQueue<HassEvent>();
        internal ConcurrentDictionary<string, HassState> FakeStates = new ConcurrentDictionary<string, HassState>();

        internal HassAreas Areas = new HassAreas();
        internal HassDevices Devices = new HassDevices();
        internal HassEntities Entities = new HassEntities();

        public HassClientMock()
        {
            // diable warnings for this method
#pragma warning disable 8619, 8620

            // Setup common mocks
            Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(true);

            SetupDefaultStates();

            SetupGet(x => x.States).Returns(FakeStates);

            Setup(x => x.GetAllStates(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => { return (IEnumerable<HassState>)FakeStates.Values; });

            Setup(x => x.ReadEventAsync())
                .ReturnsAsync(() => { return FakeEvents.TryDequeue(out var ev) ? ev : null; });

            Setup(x => x.ReadEventAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => { return FakeEvents.TryDequeue(out var ev) ? ev : null; });

            Setup(x => x.SetState(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>())).Returns<string, string, object>(
                (entityId, state, attributes) =>
                {
                    var fluentAttr = (FluentExpandoObject)attributes;
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
            );

            Setup(n => n.GetAreas()).ReturnsAsync(Areas);
            Setup(n => n.GetDevices()).ReturnsAsync(Devices);
            Setup(n => n.GetEntities()).ReturnsAsync(Entities);

            // Setup one with area
            Devices.Add(new HassDevice { Id = "device_idd", AreaId = "area_idd" });
            Areas.Add(new HassArea { Name = "Area", Id = "area_idd" });
            Entities.Add(new HassEntity
            {
                EntityId = "light.ligth_in_area",
                DeviceId = "device_idd"
            });
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

            FakeStates["media_player.player"] = new HassState
            {
                EntityId = "media_player.player",
                State = "off",
                Attributes = new Dictionary<string, object>
                {
                    ["anyattribute"] = "some attribute"
                }
            };

            FakeStates["light.ligth_in_area"] = new HassState
            {
                EntityId = "light.ligth_in_area",
                State = "off",
                Attributes = new Dictionary<string, object>
                {
                    ["anyattribute"] = "some attribute"
                }
            };
        }

        public void AddCallServiceEvent(string domain, string service, dynamic data)
        {
            // Todo: Refactor to smth smarter
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
                        EntityId = entityId,
                        State = toState,
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion"
                        }
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
                }
            });
        }

        public void AddCustomEvent(string eventType, dynamic? data)
        {
            FakeEvents.Enqueue(new HassEvent
            {
                EventType = eventType,
                Data = data
            });
        }

        public void VerifyCallService(string domain, string service,
            params (string attribute, object value)[] attributesTuples)
        {
            var attributes = new FluentExpandoObject();
            foreach (var attributesTuple in attributesTuples)
                ((IDictionary<string, object>)attributes)[attributesTuple.attribute] = attributesTuple.value;

            Verify(n => n.CallService(domain, service, attributes, It.IsAny<bool>()), Times.AtLeastOnce);
        }

        public void VerifyCallServiceTimes(string service, Times times)
        {
            Verify(n => n.CallService(It.IsAny<string>(), service, It.IsAny<FluentExpandoObject>(), It.IsAny<bool>()), times);
        }

        public void VerifySetState(string entity, string state,
            params (string attribute, object value)[] attributesTuples)
        {
            var attributes = new FluentExpandoObject();
            foreach (var attributesTuple in attributesTuples)
                ((IDictionary<string, object>)attributes)[attributesTuple.attribute] = attributesTuple.value;

            Verify(n => n.SetState(entity, state, attributes), Times.AtLeastOnce);
        }

        public void VerifySetStateTimes(string entity, Times times)
        {
            Verify(n => n.SetState(entity, It.IsAny<string>(), It.IsAny<FluentExpandoObject>()), times);
        }

        public void AssertEqual(HassState hassState, EntityState entity)
        {
            Assert.Equal(hassState.EntityId, entity.EntityId);
            Assert.Equal(hassState.State, entity.State);
            Assert.Equal(hassState.LastChanged, entity.LastChanged);
            Assert.Equal(hassState.LastUpdated, entity.LastUpdated);

            if (hassState.Attributes?.Keys == null || entity.Attribute == null)
                return;

            foreach (var attribute in hassState.Attributes!.Keys)
            {
                var attr = entity.Attribute as IDictionary<string, object> ??
                    throw new NullReferenceException($"{nameof(entity.Attribute)} catn be null");

                Assert.True(attr.ContainsKey(attribute));
                Assert.Equal(hassState.Attributes[attribute],
                    attr![attribute]);
            }
        }

        /// <summary>
        /// Gets a cancellation source that does not timeout if debugger is attached
        /// </summary>
        /// <param name="milliSeconds"></param>
        /// <returns></returns>
        public CancellationTokenSource GetSourceWithTimeout(int milliSeconds = 100)
        {
            return (Debugger.IsAttached)
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
        }
    }
}