using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public  class AutomationEntity : RxEntityBase
    {

        public AutomationEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Trigger(dynamic? data = null)
        {
            CallService("automation", "trigger", data,true);

        }

        public void Reload(dynamic? data = null)
        {
            CallService("automation", "reload", data,false);
        }
    }
}