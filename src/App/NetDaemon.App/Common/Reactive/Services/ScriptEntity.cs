using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public class ScriptEntity : RxEntityBase
    {
        public ScriptEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            CallService("script", "reload", data, false);
        }
    }
}