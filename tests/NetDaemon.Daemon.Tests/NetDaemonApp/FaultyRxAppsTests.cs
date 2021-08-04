using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Reactive.Linq;
using NetDaemon.Daemon.Fakes;
using System.Globalization;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.Daemon.Tests.NetDaemonApp
{
    public class DaemonRxAppTestApp : NetDaemon.Common.Reactive.NetDaemonRxApp { }

    public class FaultyRxAppTests : DaemonHostTestBase
    {
        public FaultyRxAppTests() : base()
        {
            App = new DaemonRxAppTestApp
            {
                Id = "id"
            };
            DefaultDaemonHost.AddRunningApp(App);
            App.StartUpAsync(DefaultDaemonHost).Wait();
        }

        public NetDaemon.Common.Reactive.NetDaemonRxApp App { get; }

        [Fact]
        public async Task ARunTimeErrorShouldLogError()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ =>
                {
                    int x = int.Parse("ss", CultureInfo.InvariantCulture);
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
        }

        [Fact]
        public async Task MissingEntityShouldNotLogError()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => App.Entity("light.do_not_exist").TurnOn());

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Never());
        }

        [Fact]
        public async Task MissingEntityShouldNotLogErrorAndNotBreakOtherApps()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            bool eventRun = false, event2Run = false;

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => eventRun = true);

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => App.Entity("light.do_not_exist").TurnOn());

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => event2Run = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Never());
            Assert.True(eventRun);
            Assert.True(event2Run);
        }

        [Fact]
        public async Task ARunTimeErrorShouldNotBreakOtherApps()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            bool eventRun = false, event2Run = false;

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => eventRun = true);

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ =>
                {
                    int x = int.Parse("ss", CultureInfo.InvariantCulture);
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => event2Run = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
            Assert.True(eventRun);
            Assert.True(event2Run);
        }

        [Fact]
        public async Task ARunTimeErrorInAttributeSelectorShouldNotBreakOtherApps()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            bool eventRun = false, event2Run = false;

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => eventRun = true);

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Where(e => e.New.Attribute!.an_int == "WTF this is not an int!!")
                .Subscribe(_ =>
                {
                    int x = int.Parse("ss", CultureInfo.InvariantCulture);
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => event2Run = true);

            AddDefaultEvent();

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
            Assert.True(eventRun);
            Assert.True(event2Run);
        }

        [Fact]
        public async Task ToUnavailableShouldNotBreak()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            bool eventRun = false, event2Run = false;

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => eventRun = true);

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Where(e => e.New.State == "on")
                .Subscribe(_ =>
                {
                    int x = int.Parse("ss", CultureInfo.InvariantCulture);
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => event2Run = true);

            AddEventFakeGoingUnavailable();

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Never());
            Assert.False(eventRun);
            Assert.False(event2Run);
        }

        [Fact]
        public async Task FromUnavailableShouldNotBreak()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            bool eventRun = false, event2Run = false;

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => eventRun = true);

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Where(e => e.New.State == "on")
                .Subscribe(_ =>
                {
                    int x = int.Parse("ss", CultureInfo.InvariantCulture);
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => event2Run = true);

            AddEventFakeFromUnavailable();

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Never());
            Assert.False(eventRun);
            Assert.False(event2Run);
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
                        EntityId = "binary_sensor.pir",
                        State = "on",
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion",
                            ["an_int"] = 10
                        },
                        LastChanged = DateTime.Now,
                        LastUpdated = DateTime.Now
                    },
                    OldState = new HassState
                    {
                        EntityId = "binary_sensor.pir",
                        State = "off",
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion",
                            ["an_int"] = 20
                        },
                        LastChanged = DateTime.Now,
                        LastUpdated = DateTime.Now
                    }
                }
            });
        }

        private void AddEventFakeFromUnavailable()
        {
            DefaultHassClientMock.FakeEvents.Enqueue(new HassEvent
            {
                EventType = "state_changed",
                Data = new HassStateChangedEventData
                {
                    EntityId = "binary_sensor.pir",
                    NewState = new HassState
                    {
                        EntityId = "binary_sensor.pir",
                        State = "on",
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion",
                            ["an_int"] = 10
                        },
                        LastChanged = DateTime.Now,
                        LastUpdated = DateTime.Now
                    },
                    OldState = null
                }
            });
        }
        private void AddEventFakeGoingUnavailable()
        {
            DefaultHassClientMock.FakeEvents.Enqueue(new HassEvent
            {
                EventType = "state_changed",
                Data = new HassStateChangedEventData
                {
                    EntityId = "binary_sensor.pir",
                    NewState = null,
                    OldState = new HassState
                    {
                        EntityId = "binary_sensor.pir",
                        State = "off",
                        Attributes = new Dictionary<string, object>
                        {
                            ["device_class"] = "motion",
                            ["an_int"] = 20
                        },
                        LastChanged = DateTime.Now,
                        LastUpdated = DateTime.Now
                    }
                }
            });
        }
    }
}