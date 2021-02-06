using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class PersistentNotificationEntity : RxEntityBase
    {
        /// <inheritdoc />
        public PersistentNotificationEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon,
            entityIds)
        {
        }

        public void Create(dynamic? data = null)
        {
            CallService("persistent_notification", "create", data, false);
        }

        public void Dismiss(dynamic? data = null)
        {
            CallService("persistent_notification", "dismiss", data, false);
        }

        public void MarkRead(dynamic? data = null)
        {
            CallService("persistent_notification", "mark_read", data, false);
        }
    }
}