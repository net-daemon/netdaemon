using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class RxAppTest : DaemonHostTestBase
    {
        public RxAppTest() : base()
        {
        }

        [Fact]
        public async Task NewAllEventDataShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateAllChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task NewEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task SameStateEventShouldNotCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "on");

            await daemonTask;

            // ASSERT
            Assert.False(called);
        }

        [Fact]
        public async Task StartupAsyncShouldThrowIfDaemonIsNull()
        {
            INetDaemonHost? host = null;

            // ARRANGE ACT ASSERT
            await Assert.ThrowsAsync<NullReferenceException>(() => DefaultDaemonRxApp.StartUpAsync(host!));
        }

        [Fact]
        public async Task StateShouldReturnCorrectEntity()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();

            // ACT
            var entity = DefaultDaemonRxApp.State("binary_sensor.pir");

            await daemonTask;

            // ASSERT
            Assert.NotNull(entity);
        }
        [Fact]
        public async Task UsingEntitiesLambdaNewEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entities(n => n.EntityId.StartsWith("binary_sensor.pir"))
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntitiesNewEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entities("binary_sensor.pir", "binary_sensor.pir_2")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntityNewEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntityNewEventShouldNotCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.other_pir")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.False(called);
        }
        [Fact]
        public async Task WhenStateStaysSameForTimeItShouldCallFunction()
        {
            var daemonTask = await GetConnectedNetDaemonTask(300);

            bool isRun = false;
            using var ctx = DefaultDaemonRxApp.StateChanges
                .Where(t => t.New.EntityId == "binary_sensor.pir")
                .NDSameStateFor(TimeSpan.FromMilliseconds(50))
                .Subscribe(e =>
                {
                    isRun = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await daemonTask;

            Assert.True(isRun);
        }
    }
}