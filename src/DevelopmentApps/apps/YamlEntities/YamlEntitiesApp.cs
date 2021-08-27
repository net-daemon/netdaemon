using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JoySoftware.HomeAssistant.Client;
using NetDaemon.Common;
using NetDaemon.Common.ModelV3;
using NetDaemon.Common.ModelV3.Domains;
using NetDaemon.Common.Reactive.Services;
using ClimateEntity = NetDaemon.Common.ModelV3.Domains.ClimateEntity;

namespace NetDaemon.DevelopmentApps.apps.YamlEntities
{
    [Focus]
    [NetDaemonApp]
    public class YamlEntitiesApp : IInitializable
    {
        public ClimateEntity TargetClimate { get; init; }
        public IEnumerable<NumericSensorEntity> TempSensors { get; init; }
        
        public YamlEntitiesApp()
        { }
        
        public void Initialize()
        {
            foreach (var numericSensorEntity in TempSensors)
            {
                numericSensorEntity.StateChanges.Subscribe(_ => Sync());
            }
        }

        private void Sync()
        {
            if (TempSensors.Any(s => s.State < 20))
            {
                TargetClimate.CallService("set_hvac_mode", new { HvacMode = "heat" });
            }
        }
    }
}