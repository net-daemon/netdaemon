using System;
using System.Linq;
using System.Reactive.Linq;
using Moq;
using NetDaemon.Daemon.Fakes;
using Xunit;

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
            VerifyEntityTurnOn("light.kitchen", new { brightness = 100 });
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
        public void TestFakeEventTurnsSetState()
        {
            // ARRANGE
            FakeMockableAppImplementation app = new(Object);
            app.Initialize();

            // ACT
            TriggerStateChange("binary_sensor.livingroom", "off", "on");
            // ASSERT
            VerifyEntitySetState("sensor.mysensor", 20);
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
            VerifyCallService(Times.Once(), "notify", "notify");
        }
    }
}