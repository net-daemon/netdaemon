using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public partial class LightEntity : RxEntityBase
    {

        public LightEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}