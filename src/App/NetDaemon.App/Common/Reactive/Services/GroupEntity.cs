using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public class GroupEntity : RxEntityBase
    {
        public GroupEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            CallService("group", "reload", data, false);
        }

        public void Set(dynamic? data = null)
        {
            CallService("group", "set", data, false);
        }

        public void Remove(dynamic? data = null)
        {
            CallService("group", "remove", data, false);
        }
    }
}