using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public partial class PersonEntity : RxEntityBase
    {
        /// <inheritdoc />
        public PersonEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Reload person data
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Reload(dynamic? data = null)
        {
            CallService("person", "reload", data, false);
        }
    }
}