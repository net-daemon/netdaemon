using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.Logging;
using Moq;
using NetDaemon.Daemon.Fakes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests.NetDaemonApp
{
    public class DaemonAppTestApp : NetDaemon.Common.NetDaemonApp { }

    public class FaultyAppTests : DaemonHostTestBase
    {
        private readonly NetDaemon.Common.NetDaemonApp _app;

        public FaultyAppTests() : base()
        {
            _app = new DaemonAppTestApp();
            _app.Id = "id";
            DefaultDaemonHost.InternalRunningAppInstances[_app.Id] = App;
            _app.StartUpAsync(DefaultDaemonHost).Wait();
        }

        public NetDaemon.Common.NetDaemonApp App => _app;

        [Fact]
        public async Task ARunTimeErrorShouldLogError()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((entity, from, to) =>
                {
                    // Do conversion error
                    int x = int.Parse("ss");
                    return Task.CompletedTask;
                }).Execute();

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
        }

        [Fact]
        public async Task ARunTimeErrorShouldNotBreakOtherApps()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            bool eventRun = false;
            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((entity, from, to) =>
                {
                    // Do conversion error
                    int x = int.Parse("ss");
                    return Task.CompletedTask;
                }).Execute();

            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((entity, from, to) =>
                {
                    // Do conversion error
                    eventRun = true;
                    return Task.CompletedTask;
                }).Execute();

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
            Assert.True(eventRun);

        }

        [Fact]
        public async Task MissingAttributeShouldNotBreakOtherApps()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            bool eventRun = false;
            App
                .Entities(e => e.Attribute!.does_not_exist == "yay")
                .WhenStateChange()
                .Call((entity, from, to) =>
                {
                    // Do conversion error
                    int x = int.Parse("ss");
                    return Task.CompletedTask;
                }).Execute();

            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((entity, from, to) =>
                {
                    // Do conversion error
                    eventRun = true;
                    return Task.CompletedTask;
                }).Execute();

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
            Assert.True(eventRun);

        }

        [Fact]
        public async Task MissingEntityShouldNotBreakOtherApps()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            bool eventRun = false;
            App
                .Entity("binary_sensor.pir")
                .WhenStateChange()
                .Call((entity, from, to) =>
                {
                    // Do conversion error
                    App.Entity("does_not_exist").TurnOn();
                    return Task.CompletedTask;
                }).Execute();

            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((entity, from, to) =>
                {
                    // Do conversion error
                    eventRun = true;
                    return Task.CompletedTask;
                }).Execute();

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            Assert.True(eventRun);

        }

        private void AddDefaultEvent()
        {
            DefaultHassClientMock.FakeEvents.Enqueue(new HassEvent
            {
                EventType = "state_changed",
                Data = new HassStateChangedEventData
                {
                    EntityId = "binary_sensor.pir",
                    NewState = new HassState
                    {
                        State = "on",
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion"
                        },
                        LastChanged = DateTime.Now,
                        LastUpdated = DateTime.Now
                    },
                    OldState = new HassState
                    {
                        State = "off",
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion"
                        }
                    }
                }
            });
        }
    }
}