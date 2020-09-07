using System;
using System.Threading.Tasks;
using Xunit;
namespace NetDaemon.Daemon.Test.Tests
{
    public class TestTheTestClass : DaemonHostTestBase
    {
        [Fact]
        public async Task TestMyApp()
        {
            var app = new RxApp { Id = "fakeId" };
            AddAppInstance(app);
            var daemon = await GetConnectedNetDaemonTask(200);
            AddChangedEvent("binary_sensor.pir", "off", "on");


            await daemon.ConfigureAwait(false);
            VerifyCallService("light", "turn_on", ("entity_id", "light.thelight"));
        }
    }
}
