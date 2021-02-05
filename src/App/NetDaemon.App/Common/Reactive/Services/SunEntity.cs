using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public  class SunEntity : RxEntityBase
    {
    
        public SunEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}