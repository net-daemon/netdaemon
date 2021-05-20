using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public partial class LockEntity : RxEntityBase
    {
        /// <inheritdoc />
        public LockEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Unlock the lock
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Unlock(dynamic? data = null)
        {
            CallService("lock", "unlock", data, true);
        }

        /// <summary>
        ///     Lock the lock
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Lock(dynamic? data = null)
        {
            CallService("lock", "lock", data, true);
        }

        /// <summary>
        ///     Open the lock
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Open(dynamic? data = null)
        {
            CallService("lock", "open", data, true);
        }
    }
}