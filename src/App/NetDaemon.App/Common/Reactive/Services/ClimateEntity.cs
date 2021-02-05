using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public class ClimateEntity : RxEntityBase
    {
        public ClimateEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void SetHvacMode(dynamic? data = null)
        {
            CallService("climate", "set_hvac_mode", data, true);
        }

        public void SetPresetMode(dynamic? data = null)
        {
            CallService("climate", "set_preset_mode", data, true);
        }

        public void SetAuxHeat(dynamic? data = null)
        {
            CallService("climate", "set_aux_heat", data, true);
        }

        public void SetTemperature(dynamic? data = null)
        {
            CallService("climate", "set_temperature", data, true);
        }

        public void SetHumidity(dynamic? data = null)
        {
            CallService("climate", "set_humidity", data, true);
        }

        public void SetFanMode(dynamic? data = null)
        {
            CallService("climate", "set_fan_mode", data, true);
        }

        public void SetSwingMode(dynamic? data = null)
        {
            CallService("climate", "set_swing_mode", true);
        }
    }
}