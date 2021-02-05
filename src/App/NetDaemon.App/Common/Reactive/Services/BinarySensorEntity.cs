using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public  class BinarySensorEntity : RxEntityBase
    {

        public BinarySensorEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}