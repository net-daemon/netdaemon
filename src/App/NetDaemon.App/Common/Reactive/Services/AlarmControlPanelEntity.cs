using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public class AlarmControlPanelEntity : RxEntityBase
    {
        public AlarmControlPanelEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void AlarmDisarm(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_disarm", data, true);
        }

        public void AlarmArmHome(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_arm_home", data, true);
        }

        public void AlarmArmAway(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_arm_away", data, true);
        }

        public void AlarmArmNight(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_arm_night", data, true);
        }

        public void AlarmArmCustomBypass(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_arm_custom_bypass", data, true);
        }

        public void AlarmTrigger(dynamic? data = null)
        {
            CallService("alarm_control_panel", "alarm_trigger", data, true);
        }
    }
}