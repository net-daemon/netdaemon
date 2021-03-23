using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class CameraEntity : RxEntityBase
    {
        /// <inheritdoc />
        public CameraEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        /// Enables motion detection
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void EnableMotionDetection(dynamic? data = null)
        {
            CallService("camera", "enable_motion_detection", data, true);
        }

        /// <summary>
        /// Disables the motion detection
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void DisableMotionDetection(dynamic? data = null)
        {
            CallService("camera", "disable_motion_detection", data, true);
        }

        /// <summary>
        /// Takes a snapshot from the camera
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void Snapshot(dynamic? data = null)
        {
            CallService("camera", "snapshot", data, true);
        }

        /// <summary>
        /// Starts playing the stream
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void PlayStream(dynamic? data = null)
        {
            CallService("camera", "play_stream", data, true);
        }

        /// <summary>
        /// Record the stream of the camera
        /// </summary>
        /// <param name="data">Additional data to pass to the service call.</param>
        public void Record(dynamic? data = null)
        {
            CallService("camera", "record", data, true);
        }
    }
}