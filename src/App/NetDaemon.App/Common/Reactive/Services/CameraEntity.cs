using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public class CameraEntity : RxEntityBase
    {
        public CameraEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void EnableMotionDetection(dynamic? data = null)
        {
            CallService("camera", "enable_motion_detection", data, true);
        }

        public void DisableMotionDetection(dynamic? data = null)
        {
            CallService("camera", "disable_motion_detection", data, true);
        }

        public void Snapshot(dynamic? data = null)
        {
            CallService("camera", "snapshot", data, true);
        }

        public void PlayStream(dynamic? data = null)
        {
            CallService("camera", "play_stream", data, true);
        }

        public void Record(dynamic? data = null)
        {
            CallService("camera", "record", true);
        }
    }
}