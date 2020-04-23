using JoySoftware.HomeAssistant.NetDaemon.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class FluentCameraTests : DaemonHostTestBase
    {
        public FluentCameraTests() : base()
        {
        }

        [Fact]
        public async Task CameraDisableMotionDetectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var entityId = "camera.camera1";
            var service_call = "disable_motion_detection";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .DisableMotionDetection()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CamerasDisableMotionDetectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var entityId = "camera.camera1";
            var service_call = "disable_motion_detection";

            // ACT
            await DefaultDaemonApp
                .Cameras(new string[] { entityId })
                .DisableMotionDetection()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CamerasFuncDisableMotionDetectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var entityId = "camera.camera1";
            var service_call = "disable_motion_detection";

            DefaultDaemonHost.InternalState["camera.camera1"] = new EntityState
            {
                EntityId = entityId,
                State = "on"
            };

            // ACT
            await DefaultDaemonApp
                    .Cameras(n => n.EntityId == entityId)
                    .DisableMotionDetection()
                    .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CameraEnableMotionDetectionCallsCorrectServiceCall()
        {
            // ARRANGE
            var entityId = "camera.camera1";
            var service_call = "enable_motion_detection";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .EnableMotionDetection()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CameraPlayStreamCallsCorrectServiceCall()
        {
            // ARRANGE
            var entityId = "camera.camera1";
            var service_call = "play_stream";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .PlayStream("media_player.player", "anyformat")
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call,
                ("media_player", "media_player.player"),
                ("format", "anyformat"),
                ("entity_id", entityId)
                );
        }

        [Fact]
        public async Task CameraRecordCallsCorrectServiceCall()
        {
            // ARRANGE
            var entityId = "camera.camera1";
            var service_call = "record";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .Record("filename", 1, 2)
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call,
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
            var entityId = "camera.camera1";
            var service_call = "snapshot";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .Snapshot("filename")
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call,
                ("filename", "filename"),
                ("entity_id", entityId)
                );
        }

        [Fact]
        public async Task CameraTurnOnCallsCorrectServiceCall()
        {
            // ARRANGE
            var entityId = "camera.camera1";
            var service_call = "turn_on";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .TurnOn()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CameraTurnOffCallsCorrectServiceCall()
        {
            // ARRANGE
            var entityId = "camera.camera1";
            var service_call = "turn_off";

            // ACT
            await DefaultDaemonApp
                .Camera(entityId)
                .TurnOff()
                .ExecuteAsync();

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes(service_call, Times.Once());
            DefaultHassClientMock.VerifyCallService("camera", service_call, ("entity_id", entityId));
        }

        [Fact]
        public async Task CamerassFuncExceptionLogsError()
        {
            // ARRANGE
            DefaultDaemonHost.InternalState["id"] = new EntityState { EntityId = "id" };

            // ACT
            var x = await Assert.ThrowsAsync<Exception>(() => DefaultDaemonApp
               .Cameras(n => throw new Exception("Some error"))
               .TurnOn()
               .ExecuteAsync());

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("turn_on", Times.Never());
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
            Assert.Equal("Some error", x.Message);
        }
    }
}