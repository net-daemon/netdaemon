using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using Moq;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Fluent;
using NetDaemon.Common.Reactive;
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
            Verify(x => x.Entity("light.kitchen").TurnOn(It.IsAny<object>()), Times.Once);
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
            Verify(x => x.CallService("notify", "notify", It.IsAny<object>()), Times.Once);
        }
    }
}