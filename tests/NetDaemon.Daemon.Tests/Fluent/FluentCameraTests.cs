using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NetDaemon.Common;
using NetDaemon.Daemon.Fakes;
using Xunit;

namespace NetDaemon.Daemon.Tests.Fluent
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class FluentCameraTests : CoreDaemonHostTestBase
    {
        public FluentCameraTests() : base()
        {
        }

        [Fact]
        public async Task CameraDisableMotionDetectionCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "disable_motion_detection";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .DisableMotionDetection()
                .ExecuteAsync()
                .ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallServiceTuple("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CamerasDisableMotionDetectionCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "disable_motion_detection";

            // ACT
            await DefaultDaemonApp
                .Cameras(new string[] { entityId })
                .DisableMotionDetection()
                .ExecuteAsync()
                .ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallServiceTuple("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CamerasFuncDisableMotionDetectionCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "disable_motion_detection";

            DefaultDaemonHost.InternalState["camera.camera1"] = new EntityState
            {
                EntityId = entityId,
                State = "on"
            };

            // ACT
            await DefaultDaemonApp
                    .Cameras(n => n.EntityId == entityId)
                    .DisableMotionDetection()
                    .ExecuteAsync()
                    .ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallServiceTuple("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CameraEnableMotionDetectionCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "enable_motion_detection";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .EnableMotionDetection()
                .ExecuteAsync().ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallServiceTuple("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CameraPlayStreamCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "play_stream";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .PlayStream("media_player.player", "anyformat")
                .ExecuteAsync().ConfigureAwait(false);

            // ASSERT
            VerifyCallServiceTimes(service_call, Times.Once());
            VerifyCallServiceTuple("camera", service_call,
                ("media_player", "media_player.player"),
                ("format", "anyformat"),
                ("entity_id", entityId)
                );
        }

        [Fact]
        public async Task CameraRecordCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "record";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .Record("filename", 1, 2)
                .ExecuteAsync().ConfigureAwait(false);

            // ASSERT
            VerifyCallServiceTimes(service_call, Times.Once());
            VerifyCallServiceTuple("camera", service_call,
                ("filename", "filename"),
                ("duration", 1),
                ("lookback", 2),
                ("entity_id", entityId)
                );
        }

        [Fact]
        public async Task CameraSnapshotCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "snapshot";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .Snapshot("filename")
                .ExecuteAsync().ConfigureAwait(false);

            // ASSERT
            VerifyCallServiceTimes(service_call, Times.Once());
            VerifyCallServiceTuple("camera", service_call,
                ("filename", "filename"),
                ("entity_id", entityId)
                );
        }

        [Fact]
        public async Task CameraTurnOnCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "turn_on";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .TurnOn()
                .ExecuteAsync().ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallServiceTuple("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CameraTurnOffCallsCorrectServiceCall()
        {
            // ARRANGE
            const string? entityId = "camera.camera1";
            const string? service_call = "turn_off";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .TurnOff()
                .ExecuteAsync().ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallServiceTuple("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        [SuppressMessage("", "CA2201")]
        public async Task CamerasFuncExceptionLogsError()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState["id"] = new EntityState { EntityId = "id" };

            // ACT
            var x = await Assert.ThrowsAsync<Exception>(() => DefaultDaemonApp
               .Cameras(_ => throw new Exception("Some error"))
               .TurnOn()
               .ExecuteAsync()).ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Never());
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
            Assert.Equal("Some error", x.Message);
        }
    }
}