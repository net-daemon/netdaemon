using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public  class BinarySensorEntity : RxEntityBase
    {
        /// <inheritdoc />
        public BinarySensorEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}