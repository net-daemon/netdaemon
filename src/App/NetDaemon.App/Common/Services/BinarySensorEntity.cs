using System;
using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Services
{
    public  class BinarySensorEntity : RxEntityBase
    {

        public BinarySensorEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}