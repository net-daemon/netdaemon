using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class LockEntity : RxEntityBase
    {
        /// <inheritdoc />
        public LockEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void Unlock(dynamic? data = null)
        {
            CallService("lock", "unlock", data, true);
        }

        public void Lock(dynamic? data = null)
        {
            CallService("lock", "lock", data, true);
        }

        public void Open(dynamic? data = null)
        {
            CallService("lock", "open", data, true);
        }
    }
}