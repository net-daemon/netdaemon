using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <summary>
    ///     Manage image processing entity
    /// </summary>
    public partial class ImageProcessingEntity : RxEntityBase
    {
        /// <inheritdoc />
        public ImageProcessingEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Scan image
        /// </summary>
        /// <param name="data">Provided data</param>
        public void Scan(dynamic? data = null)
        {
            CallService("image_processing", "scan", data, true);
        }
    }
}