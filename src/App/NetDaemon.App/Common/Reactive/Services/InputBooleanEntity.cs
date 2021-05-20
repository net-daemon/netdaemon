using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <summary>
    ///     Manage input boolean entity
    /// </summary>
    public partial class InputBooleanEntity : RxEntityBase
    {
        /// <inheritdoc />
        public InputBooleanEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Reloads input boolean
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Reload(dynamic? data = null)
        {
            CallService("input_boolean", "reload", data, false);
        }
    }
}