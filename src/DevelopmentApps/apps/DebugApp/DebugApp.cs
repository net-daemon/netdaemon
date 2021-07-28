using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Common.ModelV3;
using NetDaemon.Common.Reactive.Services;
using ZoneEntity = NetDaemon.Common.ModelV3.Domains.ZoneEntity;
using  NetDaemon.Common.ModelV3.Domains;
using ClimateEntity = NetDaemon.Common.ModelV3.Domains.ClimateEntity;

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
        private readonly ClimateEntity _climateEntity;

        public DebugApp() : base()
        {
            _climateEntity = new (this, "climate.dummy_thermostat");
        }

        public override void Initialize()
        {
            base.Initialize();
            _climateEntity.StateAllChanges.Subscribe(OnNext);

             StateChanges.Subscribe(e =>
             {
                 Console.WriteLine($"1 {e.Entity.EntityId}, {e.Old?.State} => {e.New?.State}");
             });

        }

        private void OnNext(StateChange<ClimateEntity, ClimateState> obj)
        {
            var attributes = obj.New?.Attributes;
            Console.WriteLine($"{attributes}");
            
            var t = obj.New?.Attributes.Temperature;
            
            var a2 = _climateEntity.State?.Attributes;
            
            var zone = new ZoneEntity(this, "zone.home");
            var lat = zone.State?.Attributes.latitude;
        }

    }
}
