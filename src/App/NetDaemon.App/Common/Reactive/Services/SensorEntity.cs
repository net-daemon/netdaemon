using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public  class SensorEntity : RxEntityBase
    {

        public SensorEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}