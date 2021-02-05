using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public class ZoneEntity : RxEntityBase
    {
        public ZoneEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            CallService("zone", "reload", data, false);
        }
    }
}