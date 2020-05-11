using System;
using System.Threading.Tasks;
using System.Reactive.Linq;

using Xunit;
using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;

namespace NetDaemon.Daemon.Tests.NetDaemonApp
{
    public class RxAppTests : DaemonHostTestBase
    {
        [Fact]
        public async Task TestReact()
        {
            bool isRun = false;
            using var ctx = DefaultDaemonRxApp.StateChanges
                .Where(t => t.New.EntityId == "binary_sensor.pir")
                .NDSameStateFor(TimeSpan.FromMilliseconds(50))
                .Subscribe(e =>
                {
                    isRun = true;
                });
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunDefauldDaemonUntilCanceled(500);

            Assert.True(isRun);
        }
    }

    //class TestTraceListener : TraceListener
    //{
    //    ITestOutputHelper _output;
    //    public TestTraceListener(ITestOutputHelper output) { _output = output; }
    //    public override void Write(string message) { _output.WriteLine(message); }
    //    public override void WriteLine(string message) { _output.WriteLine(message); }
    //}
}
