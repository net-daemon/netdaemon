using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public partial class SunEntity : RxEntityBase
    {
        /// <inheritdoc />
        public SunEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }
    }
}