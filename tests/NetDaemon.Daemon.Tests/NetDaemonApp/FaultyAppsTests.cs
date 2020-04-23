using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests.NetDaemonApp
{
    public class DaemonAppTestApp : JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp { }

    public class FaultyAppTests : DaemonHostTestBase
    {
        private readonly JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp _app;

        public JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp App => _app;
        public FaultyAppTests() : base()
        {

            _app = new DaemonAppTestApp();
            _app.StartUpAsync(DefaultDaemonHost);

        }

        [Fact]
        public async Task ARunTimeErrorShouldLogWarning()
        {
            // ARRANGE

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

            await RunDefauldDaemonUntilCanceled();

            LoggerMock.AssertLogged(LogLevel.Warning, Times.Once());
        }

    }
}