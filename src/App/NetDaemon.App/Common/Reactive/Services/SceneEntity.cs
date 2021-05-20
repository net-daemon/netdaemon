using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public partial class SceneEntity : RxEntityBase
    {
        /// <inheritdoc />
        public SceneEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Reload scene
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Reload(dynamic? data = null)
        {
            CallService("scene", "reload", data);
        }

        /// <summary>
        ///     Apply scene
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Apply(dynamic? data = null)
        {
            CallService("scene", "apply", data);
        }

        /// <summary>
        ///     Create scene
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Create(dynamic? data = null)
        {
            CallService("scene", "create", data);
        }
    }
}