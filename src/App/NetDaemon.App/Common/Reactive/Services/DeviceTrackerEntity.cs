using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <summary>
    ///     Manage device tracker entity
    /// </summary>
    public partial class DeviceTrackerEntity : RxEntityBase
    {
        /// <inheritdoc />
        public DeviceTrackerEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Calls see service
        /// </summary>
        /// <param name="data">Data provided to see service</param>
        public void See(dynamic? data = null)
        {
            CallService("device_tracker", "see", data, false);
        }
    }
}