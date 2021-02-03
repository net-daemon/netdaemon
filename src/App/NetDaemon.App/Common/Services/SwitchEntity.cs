using System;
using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Services
{
    public  class SwitchEntity : RxEntityBase
    {

        public SwitchEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }
}