using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    public class CoverEntity : RxEntityBase
    {
        public CoverEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void OpenCover(dynamic? data = null)
        {
            CallService("cover", "open_cover", data, true);
        }

        public void CloseCover(dynamic? data = null)
        {
            CallService("cover", "close_cover", data, true);
        }

        public void SetCoverPosition(dynamic? data = null)
        {
            CallService("cover", "set_cover_position", data, true);
        }

        public void StopCover(dynamic? data = null)
        {
            CallService("cover", "stop_cover", data, true);
        }

        public void OpenCoverTilt(dynamic? data = null)
        {
            CallService("cover", "open_cover_tilt", data, true);
        }

        public void CloseCoverTilt(dynamic? data = null)
        {
            CallService("cover", "close_cover_tilt", data, true);
        }

        public void StopCoverTilt(dynamic? data = null)
        {
            CallService("cover", "stop_cover_tilt", data, true);
        }

        public void SetCoverTiltPosition(dynamic? data = null)
        {
            CallService("cover", "set_cover_tilt_position", data, true);
        }

        public void ToggleCoverTilt(dynamic? data = null)
        {
            CallService("cover", "toggle_cover_tilt", data, true);
        }
    }
}