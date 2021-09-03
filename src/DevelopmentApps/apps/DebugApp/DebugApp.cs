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
            Console.WriteLine("isolator 1");
            StateAllChanges.Subscribe(e =>
            {
                Console.WriteLine("app 1.1");
                throw new Exception();
            });
            StateAllChanges.Subscribe(e =>
            {
                Console.WriteLine("app 1.2");
            });             }

        [HomeAssistantServiceCall]
        public void CallMeFromHass(dynamic data)
             {
            Log("A call from hass! {data}", data);
        }

        [HomeAssistantServiceCall]
        public async Task Testing(dynamic _)
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
