using System;
using System.Linq;
using System.Reactive.Linq;
using Moq;
using NetDaemon.Daemon.Fakes;
using Xunit;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Daemon.Tests.Reactive
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class RxMockTests : RxAppMock
    {
        [Fact]
        public void TestIfRxAppMockHandlesSubscriptions()
        {
            // ARRANGE
            // ACT
            Object.Entity("binary_sensor.test")
                .StateChanges
                .Where(e => e.New?.State == "on")
                .Subscribe(_ => Object.Entity("light.mylight").TurnOn());
            // ASSERT
            Verify(x => x.Entity("binary_sensor.test"), Times.Once);
            Verify(x => x.Entity(It.IsAny<string>()).TurnOn(It.IsAny<object>()), Times.Never);
        }
        [Fact]
        public void TestIfRxAppMockHandlesEventsSubscriptions()
        {
            // ARRANGE
            // ACT
            Object.Entity("binary_sensor.test")
                .StateChanges
                .Where(e => e.New?.State == "on")
                .Subscribe(_ => Object.Entity("light.mylight").TurnOn());
            // ASSERT
            Verify(x => x.Entity("binary_sensor.test"), Times.Once);
            TriggerStateChange(
                new Common.EntityState
                {
                    EntityId = "binary_sensor.test",
                    State = "off"
                },
               new Common.EntityState
               {
                   EntityId = "binary_sensor.test",
                   State = "on"
               }
            );
            Verify(x => x.Entity(It.IsAny<string>()).TurnOn(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void TestFakeMockableBinarySensorApp()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            app.Initialize();

            // ACT
            TriggerStateChange("binary_sensor.kitchen", "off", "on");

            // ASSERT
            VerifyEntityTurnOn("light.kitchen");
            VerifyEntityTurnOff("light.kitchen");
            VerifyEntityToggle("light.kitchen");
            VerifyEntityTurnOn("light.kitchen", new { brightness = 100 });
            VerifyEntityTurnOff("light.kitchen", new { brightness = 100 });
            VerifyEntityToggle("light.kitchen", new { brightness = 100 });
        }

        [Fact]
        public void TestFakeMockableBinarySensorAppEntities()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            // Have to add the entity before initialize to support lambda selections
            MockState.Add(new() { EntityId = "binary_sensor.test_entities" });

            app.Initialize();

            // ACT
            TriggerStateChange("binary_sensor.test_entities", "off", "on");

            // ASSERT
            VerifyEntityTurnOn("light.kitchen");
        }

        [Fact]
        public void TestFakeMockableGetStateForEntity()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            // Have to add the entity before initialize to support lambda selections
            MockState.Add(new() { EntityId = "sensor.some_other_entity" });
            MockState.Add(new() { EntityId = "binary_sensor.test_state_entity" });

            app.Initialize();

            // ACT
            TriggerStateChange("sensor.some_other_entity", "off", "on");
            TriggerStateChange("binary_sensor.test_state_entity", "off", "on");

            // ASSERT
            VerifyEntityTurnOn("light.state_light");
        }

        [Fact]
        public void TestFakeMockableBinarySensorAppEntitiesFalse()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            // Not end with entities
            MockState.Add(new() { EntityId = "binary_sensor.test_entity" });

            app.Initialize();

            // ACT
            TriggerStateChange("binary_sensor.test_entities", "off", "on");

            // ASSERT
            VerifyEntityTurnOn("light.kitchen", times: Times.Never());
        }

        [Fact]
        public void TestFakeEventTurnsOnLight()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            app.Initialize();

            // ACT
            TriggerEvent("hello_event", "some_domain", null);

            // ASSERT
            Verify(x => x.Entity("light.livingroom").TurnOn(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void TestFakeEvent2TurnsOnLight()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            app.Initialize();

            // ACT
            TriggerEvent(new RxEvent("hello_event", "some_domain", null));

            // ASSERT
            Verify(x => x.Entity("light.livingroom").TurnOn(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void TestFakeEventEntitySetState()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            app.Initialize();
            // ACT
            TriggerStateChange("binary_sensor.livingroom", "off", "on");
            // ASSERT
            VerifyEntitySetState("sensor.mysensor", 20);
            VerifyEntitySetState("sensor.mysensor", 20, new { battery_level = 90 });
            VerifyEntitySetState("sensor.mysensor", attributes: new { battery_level = 90 });
            VerifyEntitySetState("sensor.mysensor");
            VerifyEntitySetState("sensor.not_exist", times: Times.Never());
        }

        [Fact]
        public void TestSetState()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            // ACT
            app.Initialize();

            // ASSERT
            VerifySetState("sensor.any_sensor", 20);
            VerifySetState("sensor.any_sensor", 20, new { battery_level = 70 });
            VerifySetState("sensor.any_sensor", attributes: new { battery_level = 70 });
            VerifySetState("sensor.any_sensor");
            VerifySetState("sensor.not_exist", times: Times.Never());
        }

        [Fact]
        public void TestFakeMockableBatteryOnSensor()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            app.Initialize();

            // ACT
            TriggerStateChange(
               "sensor.temperature",
               "15",
               NewAttribute(("battery_level", 20)),
               "15",
               NewAttribute(("battery_level", 14))
            );

            // ASSERT
            VerifyCallService("notify", "notify", Times.Once());
        }
    }
}