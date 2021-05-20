using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public partial class PersistentNotificationEntity : RxEntityBase
    {
        /// <inheritdoc />
        public PersistentNotificationEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon,
            entityIds)
        {
        }

        /// <summary>
        ///     Create notification
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Create(dynamic? data = null)
        {
            CallService("persistent_notification", "create", data, false);
        }

        /// <summary>
        ///     Dismiss notification
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Dismiss(dynamic? data = null)
        {
            CallService("persistent_notification", "dismiss", data, false);
        }

        /// <summary>
        ///     Mark notification as read
        /// </summary>
        /// <param name="data">Provided data</param>
        public void MarkRead(dynamic? data = null)
        {
            CallService("persistent_notification", "mark_read", data, false);
        }
    }
}