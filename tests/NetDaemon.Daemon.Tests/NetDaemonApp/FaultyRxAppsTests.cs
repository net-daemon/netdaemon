using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Reactive.Linq;
using NetDaemon.Daemon.Fakes;

namespace NetDaemon.Daemon.Tests.NetDaemonApp
{
    public class DaemonRxAppTestApp : NetDaemon.Common.Reactive.NetDaemonRxApp { }

    public class FaultyRxAppTests : DaemonHostTestBase
    {
        private readonly NetDaemon.Common.Reactive.NetDaemonRxApp _app;

        public FaultyRxAppTests() : base()
        {
            _app = new DaemonRxAppTestApp();
            _app.Id = "id";
            DefaultDaemonHost.InternalRunningAppInstances[_app.Id] = App;
            _app.StartUpAsync(DefaultDaemonHost).Wait();
        }

        public NetDaemon.Common.Reactive.NetDaemonRxApp App => _app;

        [Fact]
        public async Task ARunTimeErrorShouldLogError()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    int x = int.Parse("ss");
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
                .Subscribe(s =>
                {
                    App.Entity("light.do_not_exist").TurnOn();
                });

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
                .Subscribe(s =>
                {
                    eventRun = true;
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    App.Entity("light.do_not_exist").TurnOn();
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    event2Run = true;
                });

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
                .Subscribe(s =>
                {
                    eventRun = true;
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    int x = int.Parse("ss");
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    event2Run = true;
                });

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
                .Subscribe(s =>
                {
                    eventRun = true;
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Where(e => e.New.Attribute!.an_int == "WTF this is not an int!!")
                .Subscribe(s =>
                {
                    int x = int.Parse("ss");
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    event2Run = true;
                });

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
                .Subscribe(s =>
                {
                    eventRun = true;
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Where(e => e.New.State == "on")
                .Subscribe(s =>
                {
                    int x = int.Parse("ss");
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    event2Run = true;
                });

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
                .Subscribe(s =>
                {
                    eventRun = true;
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Where(e => e.New.State == "on")
                .Subscribe(s =>
                {
                    int x = int.Parse("ss");
                });

            App
                .Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    event2Run = true;
                });

            AddEventFakeFromUnavailable();

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            LoggerMock.AssertLogged(LogLevel.Error, Times.Never());
            Assert.False(eventRun);
            Assert.False(event2Run);

        }

        // [Fact]
        // public async Task MissingAttributeShouldNotBreakOtherApps()
        // {
        //     // ARRANGE
        //     bool eventRun = false;
        //     App
        //         .Entities(e => e.Attribute.does_not_exist == "yay")
        //         .WhenStateChange()
        //         .Call((entity, from, to) =>
        //         {
        //             // Do conversion error
        //             int x = int.Parse("ss");
        //             return Task.CompletedTask;
        //         }).Execute();

        //     App
        //         .Entity("binary_sensor.pir")
        //         .WhenStateChange("on")
        //         .Call((entity, from, to) =>
        //         {
        //             // Do conversion error
        //             eventRun = true;
        //             return Task.CompletedTask;
        //         }).Execute();

        //     DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

        //     await RunDefauldDaemonUntilCanceled();

        //     // LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
        //     Assert.True(eventRun);

        // }

        // [Fact]
        // public async Task MissingEntityShouldNotBreakOtherApps()
        // {
        //     // ARRANGE
        //     bool eventRun = false;
        //     App
        //         .Entity("binary_sensor.pir")
        //         .WhenStateChange()
        //         .Call((entity, from, to) =>
        //         {
        //             // Do conversion error
        //             App.Entity("does_not_exist").TurnOn();
        //             return Task.CompletedTask;
        //         }).Execute();

        //     App
        //         .Entity("binary_sensor.pir")
        //         .WhenStateChange("on")
        //         .Call((entity, from, to) =>
        //         {
        //             // Do conversion error
        //             eventRun = true;
        //             return Task.CompletedTask;
        //         }).Execute();

        //     DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

        //     await RunDefauldDaemonUntilCanceled();

        //     Assert.True(eventRun);

        // }

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