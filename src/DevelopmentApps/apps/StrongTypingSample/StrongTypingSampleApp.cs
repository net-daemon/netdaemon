using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;

namespace NetDaemon.DevelopmentApps.apps.StrongTypingSample
{



    public class StrongTypingSampleApp : NetDaemonRxApp
    {
        private IEnumerable<ClimateEntity> Climates { get; init; }
        private ClimateEntity LivingRoomClimate { get; init; }

        public override void Initialize()
        {
            // Call Service Method with string argument instead of dynamic
            // (maybe an Enum would be nice, but not sure if this is a limited set)
            LivingRoomClimate.SetHvacMode(hvacMode: "heat");

            // Temperature is a strong typed double? property. 
            double? temperature = LivingRoomClimate.EntityState?.Temperature;

            // It would also be nice to have
            // LivingRoomClimate.Temperature
            // but that would require all these properties to be created on both the state and the entity
            // which seems a bit redundant and clutter the types


            // Use strong types attributes in filters
            LivingRoomClimate.StateAllChanges.Where(e => e.New.Temperature > e.Old.Temperature)
                .Subscribe(_ => Log("LivingRoom temperature is raised"));


            // or do the same for an IEnumerable<ClimateEntity>
            Climates.StateAllChanges().Where(c => c.New.Temperature > c.Old.Temperature)
                .Subscribe(t => Log($"Temperature raised to {t.New.Temperature}"));
            // unfortunately in the event handler we no longer have the reference to the entity, we could do something like


            Climates.StateAllChangesEx<ClimateEntity, ClimateEntityProperties>()
                .Where(c => c.New.Temperature > c.Old.Temperature)
                .Subscribe(c => c.Entity.SetHvacMode("on"));

            // StateAllChangesEx returns, am Observable with e tuple of (Entity, Old, New),
            // unfortunately it requires the Type arguments t be specified which makes it not as nice as it should be




            // Helper method SwitchedOn
            LivingRoomClimate.StateAllChanges.SwitchedOn().Subscribe(_ => Log("LivingRoom is Switched On"));

            // Helper method ChangedTo, means Wher(e => !e.Old.IsHeating && e.New.IsHeating)
            LivingRoomClimate.StateAllChanges.ChangedTo(s => s.IsHeating).Subscribe(_ => Log("LivingRoom started heating"));





            Climates.Any(c => c.EntityState.IsHeating);

            foreach (var climate in Climates)
            {
                climate.SetHvacMode("heat");
            }

        }

        private IObservable<(ClimateEntity Entity, ClimateEntityProperties Old, ClimateEntityProperties New)> StateAllChanges(IEnumerable<ClimateEntity> climateEntities)
        {
            return climateEntities.Select(s => s.StateAllChanges.Select(e => (Entity: s, e.Old, e.New))).Merge();
        }



        private bool SwitchedOn<TProperties>((TProperties Old, TProperties New) e)
        where TProperties : IEntityProperties
            => e.Old.State == "off" && e.New.State == "on";
    }
}
