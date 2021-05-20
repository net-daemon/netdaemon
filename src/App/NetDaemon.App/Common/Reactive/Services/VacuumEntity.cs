using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public partial class VacuumEntity : RxEntityBase
    {
        /// <inheritdoc />
        public VacuumEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Start / pause the vacuum cleaner
        /// </summary>
        /// <param name="data">Provided data</param>
        public void StartPause(dynamic? data = null)
        {
            CallService("vacuum", "start_pause", data, true);
        }

        /// <summary>
        ///     Starts the vacuum cleaner
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Start(dynamic? data = null)
        {
            CallService("vacuum", "start", data, true);
        }

        /// <summary>
        ///     Pauses the vacuum cleaner
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Pause(dynamic? data = null)
        {
            CallService("vacuum", "pause", data, true);
        }

        /// <summary>
        ///     Makes the vacuum cleaner return to base
        /// </summary>
        /// <param name="data">Provided data</param>
        public void ReturnToBase(dynamic? data = null)
        {
            CallService("vacuum", "return_to_base", data, true);
        }

        /// <summary>
        ///     Clean spot
        /// </summary>
        /// <param name="data">Provided data</param>
        public void CleanSpot(dynamic? data = null)
        {
            CallService("vacuum", "clean_spot", data, true);
        }

        /// <summary>
        ///     Locate the vacuum cleaner
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Locate(dynamic? data = null)
        {
            CallService("vacuum", "locate", data, true);
        }

        /// <summary>
        ///     Stop the vacuum cleaner
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Stop(dynamic? data = null)
        {
            CallService("vacuum", "stop", data, true);
        }

        /// <summary>
        ///     Set the vacuum cleaner fan speed
        /// </summary>
        /// <param name="data">Provided data</param>
        public void SetFanSpeed(dynamic? data = null)
        {
            CallService("vacuum", "set_fan_speed", data, true);
        }

        /// <summary>
        ///     Send command to the vacuum cleaner
        /// </summary>
        /// <param name="data">Provided data</param>
        public void SendCommand(dynamic? data = null)
        {
            CallService("vacuum", "send_command", data, true);
        }
    }
}