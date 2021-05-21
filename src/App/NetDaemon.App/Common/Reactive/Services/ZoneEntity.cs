using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public partial class ZoneEntity : RxEntityBase
    {
        /// <inheritdoc />
        public ZoneEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Reload zone
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Reload(dynamic? data = null)
        {
            CallService("zone", "reload", data, false);
        }
    }
}