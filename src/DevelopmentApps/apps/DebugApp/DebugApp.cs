using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Common.ModelV3;

namespace NetDaemon.DevelopmentApps.apps.DebugApp
{
    /// <summary> Use this class as startingpoint for debugging
    /// </summary>
    [Focus]
    public class DebugApp : NdApplication
    {
        // Use two guids, one when instanced and one when initialized
        // can track errors with instancing
        private Guid _instanceId = Guid.NewGuid();
        private readonly Entity _climateEntity;

        public DebugApp() : base()
        {
            _climateEntity = new Entity(this, "climate.dummy_thermostat");
        }

        public override void Initialize()
        {
            _climateEntity.StateAllChanges.Subscribe(OnNext);

            // StateChanges.Subscribe(e =>
            // {
            //     Console.WriteLine($"1 {e.New.EntityId} => {e.New.State}");
            // });
            //     
            // StateChanges.Subscribe(e =>
            //     {
            //         Console.WriteLine($"2 {e.New.EntityId} => {e.New.State}");
            //         throw new InvalidCastException("dsfd");
            //     });
            //
            // StateChanges.Subscribe(e =>
            // {
            //     Console.WriteLine($"2 {e.New.EntityId} => {e.New.State}");
            // });
            
            
            // var uid = Guid.NewGuid();
            // RunEvery(TimeSpan.FromSeconds(5), () => Log("Hello developer! from instance {instanceId} - {id}", _instanceId, uid));
            // CallService("notify", "persistent_notification", new { message = "Hello", title = "Yay it works!" }, true);
        }

        private void OnNext(StateChange obj)
        {
            Console.WriteLine($"From Entity {obj.Entity} {obj.New.State}");

            var x = _climateEntity.State;
        }

        // [HomeAssistantServiceCall]
        // public void CallMeFromHass(dynamic data)
        // {
        //     Log("A call from hass! {data}", data);
        // }
        //
        // [HomeAssistantServiceCall]
        // public async Task Testing(dynamic _)
        // {
        //     Log("Wait for a update");
        //     try
        //     {
        //         Entity("input_select.who_cooks").StateChanges.Timeout(TimeSpan.FromSeconds(20)).Take(1).Wait();
        //         Log("State changed as expected");
        //     }
        //     catch (System.Exception)
        //     {
        //         Log("We had timeout");
        //     }
        //     await Task.Delay(10);
        // }
    }
}
