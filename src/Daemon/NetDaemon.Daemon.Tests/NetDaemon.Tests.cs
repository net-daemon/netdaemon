using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Moq;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class NetDaemonTests
    {

        [Fact]
        public void GetStateMissingEntityReturnsNull()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            var entity = daemonHost.GetState("light.missing_entity");

            // ASSERT
            Assert.Null(entity);
        }


        [Fact]
        public void GetStateReturnsCorrectEntityState()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // Fake what is coming from hass client
            var hassState = new HassState
            {
                EntityId = "light.testlight",
                State = "on",
                LastChanged = new DateTime(2020, 1, 2, 1, 2, 3),
                LastUpdated = new DateTime(2020, 1, 2, 3, 2, 1),
                Attributes = new Dictionary<string, object>
                {
                    ["color_temp"] = 390,
                    ["brightness"] = 100,
                    ["friendly_name"] = "The test light"
                }
            };

            hcMock.FakeStates["light.testlight"] = hassState;

            // ACT
            var entity = daemonHost.GetState("light.testlight");

            // ASSERT
            hcMock.AssertEqual(hassState, entity);
        }

        [Fact]
        public async Task RunConnectedReturnsARunningTask()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);
            var cancelSource = new CancellationTokenSource();

            // ACTION
            var runTask = daemonHost.Run("localhost", 8123, false, "token", cancelSource.Token);

            await Task.Delay(10);

            // ASSERT 
            Assert.False(runTask.IsCompleted || runTask.IsCanceled);

            try
            {
                // Cleanup
                cancelSource.Cancel();
                await runTask;
            }
            catch (TaskCanceledException)
            {
                // ignore, expected behaviour
            }
        }

        [Fact]
        public async Task RunNotConnectedCompletesTask()
        {
            // ARRANGE
            // Get mock that will not connect 
            var hcMock = HassClientMock.MockConnectFalse;
            var daemonHost = new NetDaemonHost(hcMock.Object);
            var cancelSource = new CancellationTokenSource();
            // Just make sure test dont get stuck if error
            cancelSource.CancelAfter(1000);
            // ACTION
            var runTask = daemonHost.Run("localhost", 8123, false, "token", cancelSource.Token);
            await runTask;

            // ASSERT
            Assert.True(runTask.IsCompleted);
        }

        [Fact]
        public async Task RunNullReferenceToHassClientShouldThrowException()
        {
            // ARRANGE
            var daemonHost = new NetDaemonHost(null);

            // ACT and ASSERT
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
                await daemonHost.Run("", 1, false, "token", CancellationToken.None));
        }

        [Fact]
        public async Task RunWhenCanceledShouldCompleteWithCanceledException()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);
            var cancelSource = new CancellationTokenSource();

            // Wait cancel after small amount of time
            cancelSource.CancelAfter(20);

            // ACT and ASSERT
            await daemonHost.Run("", 1, false, "token", cancelSource.Token);

            Assert.False(daemonHost.Connected);
        }

        [Fact]
        public async Task TurnOnAsyncWithErrorEntityIdThrowsApplicationException()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            //
            var ex = await Assert.ThrowsAsync<ApplicationException>(async ()=>await daemonHost.TurnOnAsync("light!correct_entity"));
            
            Assert.Equal("entity_id is mal formatted light!correct_entity", ex.Message);
           
        }

        [Fact]
        public async Task TurnOnAsyncWithLightCallsSendMessageWithCorrectEntityId()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost.TurnOnAsync("light.correct_entity");

            // ASSERT
            var attributes = new ExpandoObject();
            ((IDictionary<string, object>) attributes)["entity_id"] = "light.correct_entity";
            hcMock.Verify(n => n.CallService("light", "turn_on", attributes));
        }

        [Fact]
        public async Task TurnOffAsyncWithLightCallsSendMessageWithCorrectEntityId()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost.TurnOffAsync("light.correct_entity");

            // ASSERT
            var attributes = new ExpandoObject();
            ((IDictionary<string, object>) attributes)["entity_id"] = "light.correct_entity";
            hcMock.Verify(n => n.CallService("light", "turn_off", attributes));
        }

        [Fact]
        public async Task ToggleAsyncWithLightCallsSendMessageWithCorrectEntityId()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            // ACT
            await daemonHost.ToggleAsync("light.correct_entity");

            // ASSERT
            var attributes = new ExpandoObject();
            ((IDictionary<string, object>)attributes)["entity_id"] = "light.correct_entity";
            hcMock.Verify(n => n.CallService("light", "toggle", attributes));
        }

        [Fact]
        public async Task SubscribeChangedStateForEntityWillMakeCorrectCallback()
        {
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            hcMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            CancellationTokenSource cancelSource= new CancellationTokenSource(10);
            
            string reportedState = "";

            daemonHost.ListenState("binary_sensor.pir", (entityId, newState, oldState)  =>
            {
                reportedState = newState.State;

                return Task.CompletedTask;
            });

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }
            

            Assert.Equal("on", reportedState);
        }

        [Fact]
        public async Task StopCallsCloseClient()
        {
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            await daemonHost.Stop();

            hcMock.Verify(n=>n.CloseAsync(), Times.Once);
        }
    }
}