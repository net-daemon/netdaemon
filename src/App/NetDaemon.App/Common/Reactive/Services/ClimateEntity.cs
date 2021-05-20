using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <summary>
    ///     Manage climate entity features of Home Assistant
    /// </summary>
    public class ClimateEntity : RxEntityBase
    {
        /// <inheritdoc />
        public ClimateEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Sets Hvac mode for climate entity
        /// </summary>
        /// <param name="data">Data provided</param>
        public void SetHvacMode(dynamic? data = null)
        {
            CallService("climate", "set_hvac_mode", data, true);
        }

        /// <summary>
        ///     Sets the preset mode of the climate entity
        /// </summary>
        /// <param name="data"></param>
        public void SetPresetMode(dynamic? data = null)
        {
            CallService("climate", "set_preset_mode", data, true);
        }

        /// <summary>
        ///     Sets the aux heat of the climate entity
        /// </summary>
        /// <param name="data">Data provided in the service</param>
        public void SetAuxHeat(dynamic? data = null)
        {
            CallService("climate", "set_aux_heat", data, true);
        }

        /// <summary>
        ///     Sets the temperature of the climate entity
        /// </summary>
        /// <param name="data">Data provided in the service</param>
        public void SetTemperature(dynamic? data = null)
        {
            CallService("climate", "set_temperature", data, true);
        }

        /// <summary>
        ///     Sets humidity of the climate entity
        /// </summary>
        /// <param name="data">Data provided in the service</param>
        public void SetHumidity(dynamic? data = null)
        {
            CallService("climate", "set_humidity", data, true);
        }

        /// <summary>
        ///     Sets the fan mode of the climate entity
        /// </summary>
        /// <param name="data">Data provided in the service</param>
        public void SetFanMode(dynamic? data = null)
        {
            CallService("climate", "set_fan_mode", data, true);
        }

        /// <summary>
        ///     Sets swing mode of the climate entity
        /// </summary>
        /// <param name="data">Data provided in the service</param>
        public void SetSwingMode(dynamic? data = null)
        {
            CallService("climate", "set_swing_mode", data, true);
        }
    }
}