using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Generic interface for DisableMotionDetection
    /// </summary>
    /// <typeparam name="T">Return type of operation</typeparam>
    public interface IDisableMotionDetection<T>
    {
        /// <summary>
        ///     Plays entity
        /// </summary>
        T DisableMotionDetection();
    }

    /// <summary>
    ///     Generic interface for EnableMotionDetection
    /// </summary>
    /// <typeparam name="T">Return type of operation</typeparam>
    public interface IEnableMotionDetection<T>
    {
        /// <summary>
        ///     Plays entity
        /// </summary>
        T EnableMotionDetection();
    }
    /// <summary>
    ///     Generic interface for PlayStream
    /// </summary>
    public interface IPlayStream<T>
    {
        /// <summary>
        ///     Plays a stream on media player
        /// </summary>
        /// <param name="mediaPlayerId">The media player to play camera stream to</param>
        /// <param name="format">The format that is supported of the media player</param>
        T PlayStream(string mediaPlayerId, string? format = null);
    }

    /// <summary>
    ///     Generic interface for Record
    /// </summary>
    public interface IRecord<T>
    {
        /// <summary>
        ///     Records output from camera
        /// </summary>
        /// <param name="fileName">The filename, entity_id in filename is substituted</param>
        /// <param name="seconds">Record lenght in seconds</param>
        /// <param name="lookback">Lookback time in seconds</param>
        T Record(string fileName, int? seconds = null, int? lookback = null);
    }

    /// <summary>
    ///     Generic interface for Snapshot
    /// </summary>
    public interface ISnapshot<T>
    {
        /// <summary>
        ///     Takes snapshot of current stream
        /// </summary>
        /// <param name="fileName">The filename, entity_id in filename is substituted</param>
        T Snapshot(string fileName);
    }

    /// <summary>
    ///     Fluent interface for cameras
    /// </summary>
    public interface ICamera : IExecuteAsync, IDisableMotionDetection<IExecuteAsync>, IEnableMotionDetection<IExecuteAsync>,
                                IPlayStream<IExecuteAsync>, IRecord<IExecuteAsync>, ISnapshot<IExecuteAsync>,
                                ITurnOn<IExecuteAsync>, ITurnOff<IExecuteAsync>

    { }

    /// <summary>
    ///     Implements the fluent camera interface
    /// </summary>
    public class CameraManager : EntityBase, ICamera, IExecuteAsync
    {
        private (string, dynamic?)? _serviceCall = null;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="entityIds">The unique ids of the entities managed</param>
        /// <param name="daemon">The Daemon that will handle API calls to Home Assistant</param>
        public CameraManager(IEnumerable<string> entityIds, INetDaemon daemon) : base(entityIds, daemon)
        {
        }

        /// <inheritdoc/>
        public IExecuteAsync DisableMotionDetection()
        {
            _serviceCall = ("disable_motion_detection", null);
            return this;
        }

        /// <inheritdoc/>
        public IExecuteAsync EnableMotionDetection()
        {
            _serviceCall = ("enable_motion_detection", null);
            return this;
        }

        /// <inheritdoc/>
        public IExecuteAsync PlayStream(string mediaPlayerId, string? format = null)
        {
            dynamic serviceData = new FluentExpandoObject();

            serviceData.media_player = mediaPlayerId;

            if (format is object)
                serviceData.format = format;

            _serviceCall = ("play_stream", serviceData);
            return this;
        }

        /// <inheritdoc/>
        public IExecuteAsync Record(string fileName, int? duration = null, int? lookback = null)
        {
            dynamic serviceData = new FluentExpandoObject();

            serviceData.filename = fileName;

            if (duration is object)
                serviceData.duration = duration;
            if (lookback is object)
                serviceData.lookback = lookback;

            _serviceCall = ("record", serviceData);
            return this;
        }

        /// <inheritdoc/>
        public IExecuteAsync Snapshot(string fileName)
        {
            dynamic serviceData = new FluentExpandoObject();

            serviceData.filename = fileName;

            _serviceCall = ("snapshot", serviceData);
            return this;
        }

        /// <inheritdoc/>
        public IExecuteAsync TurnOff()
        {
            _serviceCall = ("turn_off", null);
            return this;
        }

        /// <inheritdoc/>
        public IExecuteAsync TurnOn()
        {
            _serviceCall = ("turn_on", null);
            return this;
        }

        /// <inheritdoc/>
        public Task ExecuteAsync()
        {
            (string service, dynamic? data) = _serviceCall ?? throw new NullReferenceException($"{nameof(_serviceCall)} is Null!");

            return CallServiceOnAllEntities(service, data);
        }
    }

}