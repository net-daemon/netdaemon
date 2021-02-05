using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public  class SwitchEntity : RxEntityBase
    {

        public SwitchEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}