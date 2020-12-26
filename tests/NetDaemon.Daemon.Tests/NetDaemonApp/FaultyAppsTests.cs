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
        public FaultyAppTests() : base()
        {
            App = new DaemonAppTestApp
            {
                Id = "id"
            };
            DefaultDaemonHost.InternalRunningAppInstances[App.Id] = App;
            App.StartUpAsync(DefaultDaemonHost).Wait();
        }

        public NetDaemon.Common.NetDaemonApp App { get; }

        [Fact]
        public async Task ARunTimeErrorShouldLogError()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((_, _, _) =>
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
                .Call((_, _, _) =>
                {
                    // Do conversion error
                    int x = int.Parse("ss");
                    return Task.CompletedTask;
                }).Execute();

            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((_, _, _) =>
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
                .Call((_, _, _) =>
                {
                    // Do conversion error
                    int x = int.Parse("ss");
                    return Task.CompletedTask;
                }).Execute();

            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((_, _, _) =>
                {
                    // Do conversion error
                    eventRun = true;
                    return Task.CompletedTask;
                }).Execute();

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

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
                .Call((_, _, _) =>
                {
                    // Do conversion error
                    App.Entity("does_not_exist").TurnOn();
                    return Task.CompletedTask;
                }).Execute();

            App
                .Entity("binary_sensor.pir")
                .WhenStateChange("on")
                .Call((_, _, _) =>
                {
                    // Do conversion error
                    eventRun = true;
                    return Task.CompletedTask;
                }).Execute();

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            Assert.True(eventRun);
        }
    }
}