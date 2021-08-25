using System;
using System.Reactive.Linq;
using System.Text.Json;
using NetDaemon.Common;
using NetDaemon.Common.ModelV3;
using NetDaemon.Common.ModelV3.Domains;
using ZoneEntity = NetDaemon.Common.ModelV3.Domains.ZoneEntity;
using ClimateEntity = NetDaemon.Common.ModelV3.Domains.ClimateEntity;

namespace NetDaemon.DevelopmentApps.apps.M3Test
{
    [Focus]
    [NetDaemonApp]
    public class M3App
    {
        private IHaContext Ha { get; }

        private readonly ClimateEntity _climateEntity;

        public M3App(IHaContext ha)
        {
            Ha = ha;

            // Ha.CallService("notify", "persistent_notification", new { message = "Hello", title = "Yay it works in Model3 via HaContext" }, true);;

            _climateEntity = new ClimateEntity(ha, "climate.dummy_thermostat");

            _climateEntity.StateAllChanges.Where(e => e.New?.Attributes.Temperature > 20).Subscribe();
            _climateEntity.StateAllChanges.Subscribe(OnNext);

            string? state = _climateEntity.State;
            string? state2 = _climateEntity.EntityState?.State; // is the same
            DateTime? lastChanged = _climateEntity.EntityState?.LastChanged;

            Ha.StateChanges.Subscribe(e =>
            {
 
            });

            // Entity that has not changed yet is retrieved on demand
            var zone = new ZoneEntity(Ha, "zone.home");
 
            var lat = zone.Attributes?.latitude;
            
            var netEnergySensor = new NumericSensorEntity(Ha, "sensor.netto_energy");
            // NumericSensor has double? as state
            double? netEnergy = netEnergySensor.State;
            double? netEnergy2 = netEnergySensor.EntityState.State;

            netEnergySensor.StateChanges.Subscribe(e =>
                Console.WriteLine($"{e.New?.Attributes?.FriendlyName} {e.New?.State:0.##} {e.New?.Attributes?.UnitOfMeasurement}"));
            
            // Prints: 'Netto energy 8908.81 kWh'
        }

        private void OnNext(StateChange<ClimateEntity, EntityState<string, ClimateAttributes>> e)
        {
            // event has 3 properties
            ClimateEntity entity = e.Entity;
            EntityState<string, ClimateAttributes>? newSate = e.New;
            EntityState<string, ClimateAttributes>? oldState = e.Old;

            var currentState = e.Entity.EntityState; // should be the same as e.New (unless the was another change in the meantime)

            string area = e.Entity.Area;

            string? state = e.Entity.State;

            // attribute properties are strong typed
            double temperature = e.New?.Attributes.Temperature ?? 0;
            double? temperatureDelta = e.New?.Attributes.Temperature - e.Old?.Attributes.Temperature;


            // TODO: We might also want some nicer way to parse it as number (double) or On / Off
            // maybe extension methods IsOn() and IsOff() would work nice
 
            // dump as json to view the structure
            var asJson = JsonSerializer.Serialize(e, new JsonSerializerOptions(){WriteIndented = true});
            Console.WriteLine(asJson);
            
            Console.WriteLine($"{e}");


            Console.WriteLine($"{e.New?.Attributes}");
        }
    }
}