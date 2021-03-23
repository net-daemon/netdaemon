using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class AutomationEntity : RxEntityBase
    {
        /// <inheritdoc />
        public AutomationEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        /// Triggers the automation
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void Trigger(dynamic? data = null)
        {
            CallService("automation", "trigger", data, true);

        }

        /// <summary>
        /// Reloads the automation
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void Reload(dynamic? data = null)
        {
            CallService("automation", "reload", data, false);
        }
    }
}