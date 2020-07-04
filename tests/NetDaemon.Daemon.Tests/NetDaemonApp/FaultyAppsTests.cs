using Microsoft.Extensions.Logging;
using Moq;
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
            _app.StartUpAsync(DefaultDaemonHost);
        }

        public NetDaemon.Common.NetDaemonApp App => _app;

        [Fact]
        public async Task ARunTimeErrorShouldLogError()
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

            LoggerMock.AssertLogged(LogLevel.Error, Times.Once());
        }
    }
}