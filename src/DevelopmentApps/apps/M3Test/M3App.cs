using System;
using System.Reactive.Linq;
using System.Text.Json;
using NetDaemon.Common;
using NetDaemon.DevelopmentApps.Domains.Climate;
using NetDaemon.DevelopmentApps.Domains.Sensor;
using NetDaemon.DevelopmentApps.Domains.Zone;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.DevelopmentApps.apps.M3Test
{
    [NetDaemonApp]
    public class M3App
    {
        private IHaContext Ha { get; }

        private readonly ClimateEntity _climateEntity;

        public M3App(IHaContext ha)
        {
            Ha = ha;

            // Ha.CallService("notify", "persistent_notification", new { message = "Hello", title = "Yay it works in HassModel via HaContext" }, true);;

            _climateEntity = new ClimateEntity(ha, "climate.dummy_thermostat");
            
            _climateEntity.StateAllChanges().Where(e => e.New?.Attributes?.Temperature > 20).Subscribe();
            _climateEntity.StateAllChanges().Subscribe(OnNext);

            string? state = _climateEntity.State;
            string? state2 = _climateEntity.EntityState?.State; // is the same
            DateTime? lastChanged = _climateEntity.EntityState?.LastChanged;

            Ha.StateChanges().Subscribe(e =>
            {
 
            });

            // Entity that has not changed yet is retrieved on demand
            var zone = new ZoneEntity(Ha, "zone.home");
 
            var lat = zone.Attributes?.latitude;
            
            var netEnergySensor = new NumericSensorEntity(Ha, "sensor.netto_energy");
            // NumericSensor has double? as state
            double? netEnergy = netEnergySensor.State;
            double? netEnergy2 = netEnergySensor.EntityState?.State;

            netEnergySensor.StateChanges().Subscribe(e =>
                Console.WriteLine($"{e.New?.Attributes?.FriendlyName} {e.New?.State} {e.New?.Attributes?.UnitOfMeasurement}"));
            
            // Prints: 'Netto energy 8908.81 kWh'
        }

        private void OnNext(StateChange<ClimateEntity, EntityState<ClimateAttributes>> e)
        {
            // event has 3 properties
            ClimateEntity entity = e.Entity;
            EntityState<ClimateAttributes>? newSate = e.New;
            EntityState<ClimateAttributes>? oldState = e.Old;

            var currentState = e.Entity.EntityState; // should be the same as e.New (unless the was another change in the meantime)

            string? state = e.Entity.State;

            // attribute properties are strong typed
            double temperature = e.New?.Attributes?.Temperature ?? 0;
            double? temperatureDelta = e.New?.Attributes?.Temperature - e.Old?.Attributes?.Temperature;


            // dump as json to view the structure
            var asJson = JsonSerializer.Serialize(e, new JsonSerializerOptions(){WriteIndented = true});
            Console.WriteLine(asJson);
            
            Console.WriteLine($"{e}");


            Console.WriteLine($"{e.New?.Attributes}");
        }
    }
}