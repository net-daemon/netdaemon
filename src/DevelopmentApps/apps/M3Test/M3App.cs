using System;
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

            _climateEntity.StateAllChanges.Subscribe(ClimateStateChanged);

            Ha.StateChanges.Subscribe(e =>
            {
                Console.WriteLine($"1 {e.Entity.EntityId}, {e.Old?.State} => {e.New?.State}");
            });

            // Entity that has not changed yet is retrieved on demand
            var zone = new ZoneEntity(Ha, "zone.home");
            var lat = zone.State?.Attributes.latitude;
        }

        private void ClimateStateChanged(StateChange<ClimateEntity, ClimateState> e)
        {
            // event has 3 properties
            ClimateEntity entity = e.Entity;
            ClimateState? newSate = e.New;
            ClimateState? oldState = e.Old;

            ClimateState? currentState = e.Entity.State; // should be the same as new

            string area = e.Entity.Area;
            ClimateState? state = e.Entity.State;

            // attribute properties are strong typed
            double temperature = state?.Attributes.Temperature ?? 0;
            double? temperatureDelta = e.New?.Attributes.Temperature - e.Old?.Attributes.Temperature;

            // The 'entity state' is a bit weird to give a good name
            // We have the 'state object' which is a class that has a property for the 'state' which is a string 
            string? entityState = state?.State;

            // TODO: We might also want some nicer way to parse it as number (double) or On / Off
            // maybe extension methods IsOn() and IsOff() would work nice

            Console.WriteLine($"{e}");


            Console.WriteLine($"{e.New?.Attributes}");
        }
    }
}