using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <summary>
    ///  Basic Cover Entity
    /// </summary>
    public class CoverEntity : RxEntityBase
    {
        /// <inheritdoc />
        public CoverEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        /// Opens the cover
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void OpenCover(dynamic? data = null)
        {
            CallService("cover", "open_cover", data, true);
        }

        /// <summary>
        /// Closes the cover
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void CloseCover(dynamic? data = null)
        {
            CallService("cover", "close_cover", data, true);
        }

        /// <summary>
        /// Sets the cover to a given position
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void SetCoverPosition(dynamic? data = null)
        {
            CallService("cover", "set_cover_position", data, true);
        }

        /// <summary>
        /// Stops the cover
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void StopCover(dynamic? data = null)
        {
            CallService("cover", "stop_cover", data, true);
        }

        /// <summary>
        /// Opens the cover tilted
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void OpenCoverTilt(dynamic? data = null)
        {
            CallService("cover", "open_cover_tilt", data, true);
        }

        /// <summary>
        /// Closes the cover tilted
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void CloseCoverTilt(dynamic? data = null)
        {
            CallService("cover", "close_cover_tilt", data, true);
        }

        /// <summary>
        /// Stops the cover tilting
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void StopCoverTilt(dynamic? data = null)
        {
            CallService("cover", "stop_cover_tilt", data, true);
        }

        /// <summary>
        /// Sets the cover tilted position
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void SetCoverTiltPosition(dynamic? data = null)
        {
            CallService("cover", "set_cover_tilt_position", data, true);
        }

        /// <summary>
        /// Toggles the cover tilting
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void ToggleCoverTilt(dynamic? data = null)
        {
            CallService("cover", "toggle_cover_tilt", data, true);
        }
    }
}