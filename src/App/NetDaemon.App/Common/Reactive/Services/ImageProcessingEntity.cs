using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class ImageProcessingEntity : RxEntityBase
    {
        /// <inheritdoc />
        public ImageProcessingEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void Scan(dynamic? data = null)
        {
            CallService("image_processing", "scan", data, true);
        }
    }
}