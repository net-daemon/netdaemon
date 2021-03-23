using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class VacuumEntity : RxEntityBase
    {
        /// <inheritdoc />
        public VacuumEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void StartPause(dynamic? data = null)
        {
            CallService("vacuum", "start_pause", data, true);
        }

        public void Start(dynamic? data = null)
        {
            CallService("vacuum", "start", data, true);
        }

        public void Pause(dynamic? data = null)
        {
            CallService("vacuum", "pause", data, true);
        }

        public void ReturnToBase(dynamic? data = null)
        {
            CallService("vacuum", "return_to_base", true);
        }

        public void CleanSpot(dynamic? data = null)
        {
            CallService("vacuum", "clean_spot", data, true);
        }

        public void Locate(dynamic? data = null)
        {
            CallService("vacuum", "locate", data, true);
        }

        public void Stop(dynamic? data = null)
        {
            CallService("vacuum", "stop", data, true);
        }

        public void SetFanSpeed(dynamic? data = null)
        {
            CallService("vacuum", "set_fan_speed", data, true);
        }

        public void SendCommand(dynamic? data = null)
        {
            CallService("vacuum", "send_command", data, true);
        }
    }
}