using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;

namespace NetDaemon.DevelopmentApps.apps.DebugApp
{
    /// <summary> Use this class as startingpoint for debugging
    /// </summary>
    public class DebugApp : NetDaemonRxApp
    {
        // Use two guids, one when instanced and one when initialized
        // can track errors with instancing
        private Guid _instanceId = Guid.NewGuid();
        public DebugApp() : base()
        {
        }

        public override void Initialize()
        {
            var uid = Guid.NewGuid();
            RunEvery(TimeSpan.FromSeconds(5), () => Log("Hello developer! from instance {instanceId} - {id}", _instanceId, uid));
            // RunIn(TimeSpan.FromSeconds(1), () => SetState("sensor.awesome", 15, new { display_name = "kalle" }, waitForResponse: false));
            var x = SetState("sensor.awesome", 15, new { display_name = "kalle" }, waitForResponse: true);
            // Log($"State: {x?.State}");
            // RunIn(TimeSpan.FromSeconds(1), () =>
            // {
            //     Log("Wait for a update2");
            //     Entity("input_select.who_cooks").StateChanges.Timeout(TimeSpan.FromSeconds(20)).Take(1).Wait();
            //     Log("State changed as expected2");
            // });
        }

        [HomeAssistantServiceCall]
        public void CallMeFromHass(dynamic data)
        {
            Log("A call from hass! {data}", data);
        }

        [HomeAssistantServiceCall]
        public async Task Testing(dynamic data)
        {
            Log("Wait for a update");
            try
            {
                Entity("input_select.who_cooks").StateChanges.Timeout(TimeSpan.FromSeconds(20)).Take(1).Wait();
                Log("State changed as expected");
            }
            catch (System.Exception)
            {
                Log("We had timeout");
            }
            await Task.Delay(10);
        }
    }
}
