using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public  class ImageProcessingEntity : RxEntityBase
    {

        public ImageProcessingEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Scan(dynamic? data = null)
        {
            CallService("image_processing", "scan", data,true);
        }
    }
}