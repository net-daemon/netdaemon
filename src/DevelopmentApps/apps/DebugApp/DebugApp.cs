using System;
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
        }

        [HomeAssistantServiceCall]
        public void CallMeFromHass(dynamic data)
        {
            Log("A call from hass! {data}", data);
        }
    }
}
