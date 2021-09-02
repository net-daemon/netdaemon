using System;
using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common;
using NetDaemon.Model3.Domains.Climate;
using NetDaemon.Model3.Domains.Sensor;

namespace NetDaemon.DevelopmentApps.apps.YamlEntities
{
    [Focus]
    [NetDaemonApp]
    public class YamlEntitiesApp : IInitializable
    {
        public ClimateEntity? TargetClimate { get; init; }
        public IEnumerable<NumericSensorEntity> TempSensors { get; init; } = Array.Empty<NumericSensorEntity>();
        
        public void Initialize()
        {
            foreach (var numericSensorEntity in TempSensors)
            {
                numericSensorEntity.StateChanges.Subscribe(_ => Sync());
            }
        }

        private void Sync()
        {
            if (TempSensors?.Any(s => s.State < 20) ?? false)
            {
                TargetClimate?.CallService("set_hvac_mode", new { HvacMode = "heat" });
            }
        }
    }
}