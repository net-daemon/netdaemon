using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class InputBooleanEntity : RxEntityBase
    {
        /// <inheritdoc />
        public InputBooleanEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            CallService("input_boolean", "reload", data, false);
        }
    }
}