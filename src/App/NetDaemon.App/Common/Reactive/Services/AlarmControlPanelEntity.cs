using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class AlarmControlPanelEntity : RxEntityBase
    {
        /// <inheritdoc />
        public AlarmControlPanelEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        /// Disarms the alarm
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void AlarmDisarm(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_disarm", data, true);
        }

        /// <summary>
        /// Arms the alarm when for home
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void AlarmArmHome(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_arm_home", data, true);
        }

        /// <summary>
        /// Arms the alarm for away
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void AlarmArmAway(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_arm_away", data, true);
        }

        /// <summary>
        /// Arms the alarm for night
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void AlarmArmNight(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_arm_night", data, true);
        }

        /// <summary>
        /// Arms the alarm for custom bypass
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void AlarmArmCustomBypass(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_arm_custom_bypass", data, true);
        }

        /// <summary>
        /// Triggers the alarm
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void AlarmTrigger(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_trigger", data, true);
        }
    }
}