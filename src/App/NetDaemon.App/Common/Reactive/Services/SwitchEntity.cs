using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public  class SwitchEntity : RxEntityBase
    {
        /// <inheritdoc />
        public SwitchEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}