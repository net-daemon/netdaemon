using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public record ClimateEntityProperties : EntityPropertyBase
    {

        public ClimateEntityProperties(IEntityProperties innerEntityProperties) : base(innerEntityProperties)
        {
        }

        // This is like a derived property
        public bool IsHeating => Attribute?.hvac_action == "heating";
        
        // The State but now as a string (maybe an on/off enum?)
        public new string? State => base.State?.ToString();

        
        // map the attributes
        public double? Temperature => Attribute?.temperature as double?;
        
        public double? CurrentTemperature => Attribute?.current_temperature as double?;
        
        public string? HvacAction => Attribute?.hvac_action;
    }

    /// <inheritdoc />
    public class ClimateEntity : RxEntityBase<ClimateEntityProperties>
    {
        /// <inheritdoc />
        public ClimateEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void SetHvacMode(string hvacMode)
        {
            SetHvacMode(new { hvac_mode = hvacMode });
        }

        /// <summary>
        ///     Sets Hvac mode for climate entity
        /// </summary>
        /// <param name="data">Data provided</param>
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
            CallService("climate", "set_swing_mode", data, true);
        }

        protected override ClimateEntityProperties MapEntityState(EntityState state) => new ClimateEntityProperties(state);
    }
}