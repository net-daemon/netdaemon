using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public  class DeviceTrackerEntity : RxEntityBase
    {
 
        public DeviceTrackerEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void See(dynamic? data = null)
        {
           CallService("device_tracker", "see", data,false);
        }
    }
}