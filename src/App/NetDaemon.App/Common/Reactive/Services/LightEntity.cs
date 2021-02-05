using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class LightEntity : RxEntityBase
    {
        /// <inheritdoc />
        public LightEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}