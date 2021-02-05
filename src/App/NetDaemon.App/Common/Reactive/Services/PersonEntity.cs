using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public class PersonEntity : RxEntityBase
    {
        public PersonEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            CallService("person", "reload", data, false);
        }
    }
}