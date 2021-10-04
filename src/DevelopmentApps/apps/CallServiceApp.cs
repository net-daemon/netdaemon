using System.Text.Json.Serialization;
using NetDaemon.Common;
using NetDaemon.DevelopmentApps.Domains.Climate;
using NetDaemon.Model3.Common;


namespace NetDaemon.DevelopmentApps.apps
{
    [NetDaemonApp]
    public class CallServiceApp
    {
        public CallServiceApp(IHaContext ha)
        {
            var climate = new ClimateEntity(ha, "climate.dummy_thermostat");
            
            climate.CallService("set_temperature",
                new SetTemperatureData
                {
                    HvacMode = "heat",
                    Temperature = 20,
                }
            );
        }

        record SetTemperatureData
        {
            [JsonPropertyName("temperature")] public double? Temperature { get; init; }
            [JsonPropertyName("target_temp_high")] public double? TargetTempHigh { get; init; }
            [JsonPropertyName("target_temp_low")] public double? TargetTempLow { get; init; }
            [JsonPropertyName("hvac_mode")] public string? HvacMode { get; init; }
        }
    }
}