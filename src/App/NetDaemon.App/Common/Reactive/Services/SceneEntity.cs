using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class SceneEntity : RxEntityBase
    {
        /// <inheritdoc />
        public SceneEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            CallService("scene", "reload", data);
        }

        public void Apply(dynamic? data = null)
        {
            CallService("scene", "apply", data);
        }

        public void Create(dynamic? data = null)
        {
            CallService("scene", "create", data);
        }
    }
}