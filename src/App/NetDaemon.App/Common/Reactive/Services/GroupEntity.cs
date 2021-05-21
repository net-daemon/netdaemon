using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <summary>
    ///     Manage group entity
    /// </summary>
    public partial class GroupEntity : RxEntityBase
    {
        /// <inheritdoc />
        public GroupEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Reload group
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Reload(dynamic? data = null)
        {
            CallService("group", "reload", data, false);
        }

        /// <summary>
        ///     Set group
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Set(dynamic? data = null)
        {
            CallService("group", "set", data, false);
        }

        /// <summary>
        ///     Remove group
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Remove(dynamic? data = null)
        {
            CallService("group", "remove", data, false);
        }
    }
}