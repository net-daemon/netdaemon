using JoySoftware.HomeAssistant.Client;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Common;
using Xunit;
using NetDaemon.Daemon.Fakes;

namespace NetDaemon.Daemon.Tests.Daemon
{
    public class HostTestApp : NetDaemon.Common.NetDaemonApp
    {
    }

    public class NetDaemonTests : CoreDaemonHostTestBase
    {
        public NetDaemonTests() : base()
        {
        }

        [Fact]
        public async Task EventShouldCallCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);


            dynamic helloWorldDataObject = GetDynamicDataObject(HelloWorldData);

            DefaultHassClientMock.AddCustomEvent("CUSTOM_EVENT", helloWorldDataObject);

            var isCalled = false;
            var message = "";

            // ACT
            DefaultDaemonApp.ListenEvent("CUSTOM_EVENT", (ev, data) =>
            {
                isCalled = true;
                message = data.Test;
                return Task.CompletedTask;
            });

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(isCalled);
            Assert.Equal(HelloWorldData, message);
        }

        [Fact]
        public async Task AttributeServiceCallShouldFindCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var app = new AssmeblyDaemonApp();
            app.Id = "id";


            DefaultDaemonHost.InternalRunningAppInstances[app.Id] = app;

            // ACT
            await app.HandleAttributeInitialization(DefaultDaemonHost).ConfigureAwait(false);
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Single(app.DaemonCallBacksForServiceCalls);
        }

        [Fact]
        public void GetStateMissingEntityReturnsNull()
        {
            // ARRANGE

            // ACT
            var entity = DefaultDaemonHost.GetState("light.missing_entity");

            // ASSERT
            Assert.Null(entity);
        }

        [Fact]
        public async Task GetStateReturnsCorrectEntityState()
        {
            // ARRANGE

            // Fake what is coming from hass client
            await InitializeFakeDaemon().ConfigureAwait(false);

            // ACT
            var entity = DefaultDaemonHost.GetState("light.correct_entity");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(entity);
            Assert.Equal("on", entity?.State);
        }

        // Todo: Add tests to test objects and arrays from the dynamic conversion

        [Fact]
        public async Task OtherEventShouldNotCallCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            dynamic dataObject = GetDynamicDataObject();

            DefaultHassClientMock.AddCustomEvent("CUSTOM_EVENT", dataObject);

            var isCalled = false;

            // ACT
            DefaultDaemonApp.ListenEvent("OTHER_EVENT", (ev, data) =>
            {
                isCalled = true;
                return Task.CompletedTask;
            });

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(isCalled);
        }

        [Fact]
        public async Task RunNotConnectedCompletesTask()
        {
            // ARRANGE

            // ACTION
            var (runTask, _) = ReturnRunningNotConnectedDaemonHostTask();
            await runTask;

            // ASSERT
            Assert.True(runTask.IsCompleted);
        }

        [Fact]
        public void RunNullReferenceToHassClientShouldThrowException()
        {
            // ARRANGE

            // ACT and ASSERT
            Assert.Throws<ArgumentNullException>(() =>
                { var DefaultDaemonHost = new NetDaemonHost(null, null); });
        }

        [Fact]
        public async Task SendEventShouldCallCorrectMethod()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var eventData = GetDynamicDataObject();


            // ACT
            await DefaultDaemonHost.SendEvent("test_event", eventData);

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            var expandoObject = (ExpandoObject)eventData;
            VerifyEventSent("test_event", expandoObject);
        }

        [Fact]
        public async Task SendEventWithNullDataShouldCallCorrectMethod()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            await DefaultDaemonHost.SendEvent("test_event");
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            VerifyEventSent("test_event");
        }

        [Fact]
        public async Task SpeakShouldCallCorrectService()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ARRANGE
            SetEntityState(new()
            {
                EntityId = "media_player.fakeplayer"
            });

            DefaultDaemonHost.InternalDelayTimeForTts = 0; // For testing

            // ACT
            DefaultDaemonHost.Speak("media_player.fakeplayer", "Hello test!");

            var (_, expObject) = GetDynamicObject(
                ("entity_id", "media_player.fakeplayer"),
                ("message", "Hello test!")
            );
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            VerifyCallService("tts", "google_cloud_say", expObject, true);
        }

        [Fact]
        public async Task SpeakShouldWaitUntilMediaPlays()
        {
            // ARRANGE
            SetEntityState(new()
            {
                EntityId = "media_player.fakeplayer",
                Attributes = new()
                {
                    ["entity_id"] = "media_player.fakeplayer",
                    ["message"] = "Hello test!",
                    ["media_duration"] = 0.2
                }
            }
            );

            // Get a running default Daemon
            await InitializeFakeDaemon(500).ConfigureAwait(false);

            // Expected data call service
            var (expectedAttruibutes, expectedAttributesExpObject) = GetDynamicObject(
                ("entity_id", "media_player.fakeplayer"),
                ("message", "Hello test!")
            );

            // ACT
            DefaultDaemonHost.Speak("media_player.fakeplayer", "Hello test!");
            DefaultDaemonHost.Speak("media_player.fakeplayer", "Hello test!");

            // ASSERT

            await Task.Delay(50);

            VerifyCallService("tts", "google_cloud_say", expectedAttributesExpObject, true, Times.Once());

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);
            // Called twice
            VerifyCallService("tts", "google_cloud_say", expectedAttributesExpObject, true, Times.Exactly(2));

        }

        [Fact]
        public async Task StopCallsCloseClient()
        {
            await DefaultDaemonHost.Stop();

            DefaultHassClientMock.Verify(n => n.CloseAsync(), Times.Once);
        }

        [Fact]
        public async Task SubscribeChangedStateForEntityWillMakeCorrectCallback()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            string? reportedState = "";

            // ACT
            DefaultDaemonApp.ListenState("binary_sensor.pir", (entityId, newState, oldState) =>
            {
                reportedState = newState?.State;

                return Task.CompletedTask;
            });

            AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Equal("on", reportedState);
        }

        [Fact]
        public async Task SubscribeChangedStateForAllChangesWillMakeCorrectCallbacks()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            int nrOfTimesCalled = 0;

            // ACT
            DefaultDaemonApp.ListenState("", (entityId, newState, oldState) =>
            {
                nrOfTimesCalled++;

                return Task.CompletedTask;
            });

            AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");
            AddChangedEvent("light.mylight", fromState: "on", toState: "off");

            await RunFakeDaemonUntilTimeout();

            // ASSERT
            Assert.Equal(2, nrOfTimesCalled);
        }

        [Fact]
        public async Task ChangedEventHaveNullDataShouldThrowException()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            DefaultHassClientMock.FakeEvents.Enqueue(new HassEvent
            {
                EventType = "state_changed",
                Data = null
            });

            // ACT
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            //ASSERT
            LoggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
        }

        [Fact]
        public async Task CancelChangedStateForSubscriptionWillNotMakeCallback()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            bool isCalled = false;

            // ACT
            var id = DefaultDaemonApp.ListenState("binary_sensor.pir", (entityId, newState, oldState) =>
            {
                isCalled = true;

                return Task.CompletedTask;
            });

            DefaultDaemonApp.CancelListenState(id!);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(isCalled);
            Assert.Empty(DefaultDaemonApp.InternalStateActions);
        }

        [Fact]
        public async Task CallServiceEventShouldCallCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var dynObject = GetDynamicDataObject(HelloWorldData);


            var isCalled = false;
            string? message = "";

            // ACT
            DefaultDaemonHost.ListenServiceCall("custom_domain", "any_service", data =>
            {
                isCalled = true;
                message = data?.Test;
                return Task.CompletedTask;
            });

            DefaultHassClientMock.AddCallServiceEvent("custom_domain", "any_service", dynObject);

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(isCalled);
            Assert.Equal(HelloWorldData, message);
        }

        [Fact]
        public async Task CallServiceEventOtherShouldNotCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var dynObject = GetDynamicDataObject(HelloWorldData);

            DefaultHassClientMock.AddCallServiceEvent("custom_domain", "other_service", dynObject);

            var isCalled = false;
            string? message = "";

            DefaultDaemonHost.ListenServiceCall("custom_domain", "any_service", data =>
            {
                isCalled = true;
                message = data?.Test;
                return Task.CompletedTask;
            });

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            Assert.False(isCalled);
            Assert.True(string.IsNullOrEmpty(message));
        }

        [Fact]
        public async Task SetStateShouldCallCorrectFunction()
        {
            await DefaultDaemonHost.SetStateAsync("sensor.any_sensor", "on", ("attr", "value"));

            var (dynObj, expObj) = GetDynamicObject(
                ("attr", "value")
            );
            DefaultHassClientMock.Verify(n => n.SetState("sensor.any_sensor", "on", expObj));
        }

        [Fact]
        public async Task SetStateShouldReturnCorrectData()
        {
            await DefaultDaemonHost.SetStateAsync("sensor.any_sensor", "on", ("attr", "value"));

            var (dynObj, expObj) = GetDynamicObject(
                ("attr", "value")
            );
            DefaultHassClientMock.Verify(n => n.SetState("sensor.any_sensor", "on", expObj));
        }

        [Fact]
        public async Task DelayStateChangeShouldReturnTrue()
        {
            // ARRANGE

            // ACT
            await InitializeFakeDaemon().ConfigureAwait(false);
            using var delayResult = DefaultDaemonApp.DelayUntilStateChange(new string[] { "binary_sensor.pir" }, to: "on");

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);
            // ASSERT
            Assert.True(delayResult.Task.Result);
            Assert.Empty(DefaultDaemonApp.InternalStateActions);
        }

        [Fact]
        public async Task DelayStateChangeWithToAndFromShouldReturnTrue()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            using var delayResult = DefaultDaemonApp.DelayUntilStateChange(new string[] { "binary_sensor.pir" }, to: "on", from: "off");

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(delayResult.Task.Result);
            Assert.Empty(DefaultDaemonApp.InternalStateActions);
        }

        [Fact]
        public async Task DelayStateChangeWithToAndFromWrongShouldNotComplete()
        {
            // ARRANGE  
            await InitializeFakeDaemon().ConfigureAwait(false);

            // ACT
            using var delayResult = DefaultDaemonApp.DelayUntilStateChange(new string[] { "binary_sensor.pir" }, to: "on", from: "unknown");

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(delayResult.Task.IsCompleted);
            Assert.Single(DefaultDaemonApp.InternalStateActions);
        }

        [Fact]
        public async Task DelayStateLambdaChangeShouldReturnTrue()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            using var delayResult = DefaultDaemonApp.DelayUntilStateChange(new string[] { "binary_sensor.pir" }, (n, o) => n?.State == "on");

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(delayResult.Task.Result);
            Assert.Empty(DefaultDaemonApp.InternalStateActions);
        }

        [Fact]
        public async Task DelayStateLambdaChangeShouldNotComplete()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            using var delayResult = DefaultDaemonApp.DelayUntilStateChange(new string[] { "binary_sensor.pir" }, (n, o) => n?.State == "on");

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", fromState: "on", toState: "off");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(delayResult.Task.IsCompleted);
            Assert.Single(DefaultDaemonApp.InternalStateActions);
        }

        [Fact]
        public async Task DelayStateChangeCancelShouldReturnFalse()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            // ACT
            using var delayResult = DefaultDaemonApp.DelayUntilStateChange(new string[] { "binary_sensor.pir" }, to: "on");

            delayResult.Cancel();

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(delayResult.Task.Result);
            Assert.Empty(DefaultDaemonApp.InternalStateActions);
        }

        [Fact]
        public void GetAppOnMissingAppShouldReturnNull()
        {
            // ARRANGE
            // ACT
            var theApp = DefaultDaemonHost.GetApp("noexist");

            // ASSERT
            Assert.Null(theApp);
        }

        [Fact]
        public async Task ClearShouldReturnNullGetApp()
        {
            // ARRANGE
            var theApp = DefaultDaemonHost.GetApp("app_id");
            Assert.NotNull(theApp);

            // ACT
            await DefaultDaemonHost.UnloadAllApps().ConfigureAwait(false);
            theApp = DefaultDaemonHost.GetApp("app_id");

            // ASSERT
            Assert.Null(theApp);
        }

        [Fact]
        public void EntityShouldReturCorrectValueForArea()
        {
            // ARRANGE
            DefaultDaemonHost._hassDevices["device_id"] = new HassDevice { AreaId = "area_id" };
            DefaultDaemonHost._hassAreas["area_id"] = new HassArea { Name = "Correct name", Id = "area_id" };
            DefaultDaemonHost._hassEntities["light.lamp"] = new HassEntity
            {
                EntityId = "light.lamp",
                DeviceId = "device_id"
            };
            // ACT
            var areaName = DefaultDaemonHost.GetAreaForEntityId("light.lamp");

            // ASSERT
            Assert.Equal("Correct name", areaName);
        }

        [Fact]
        public void EntityShouldReturNullForAreaNotExist()
        {
            // ARRANGE
            DefaultDaemonHost._hassDevices["device_id"] = new HassDevice { AreaId = "area_id" };
            DefaultDaemonHost._hassAreas["area_id"] = new HassArea { Name = "Correct name", Id = "area_id" };
            DefaultDaemonHost._hassEntities["light.lamp"] = new HassEntity
            {
                EntityId = "light.lamp",
                DeviceId = "device_id"
            };
            // ACT
            var areaName = DefaultDaemonHost.GetAreaForEntityId("light.not_exist_lamp");

            // ASSERT
            Assert.Null(areaName);
        }

        [Fact]
        public async Task StateChangeHasAreaInformation()
        {
            // ARRANGE
            DefaultDaemonHost._hassDevices["device_id"] = new HassDevice { AreaId = "area_id" };
            DefaultDaemonHost._hassAreas["area_id"] = new HassArea { Name = "Correct name", Id = "area_id" };
            DefaultDaemonHost._hassEntities["binary_sensor.pir"] = new HassEntity
            {
                EntityId = "binary_sensor.pir",
                DeviceId = "device_id"
            };
            await InitializeFakeDaemon().ConfigureAwait(false);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", fromState: "off", toState: "on");

            // ACT

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Equal("Correct name", DefaultDaemonHost.InternalState["binary_sensor.pir"].Area);
        }

        [Fact]
        public async Task SetStateShouldKeepSameArea()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            var state = await DefaultDaemonHost.SetStateAsync("light.ligth_in_area", "on", ("attr", "value"));
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);
            /// ASSERT
            Assert.Equal("Area", state?.Area);
        }

        [Theory]
        [InlineData(false, null, null, null, null)]
        [InlineData(true, null, 10, null, 10)]
        [InlineData(true, 10, null, 10, null)]
        [InlineData(true, "unavailable", 10, null, 10)]
        [InlineData(true, 10, "unavailable", 10, null)]
        [InlineData(true, 10, 11, 10, 11)]
        [InlineData(true, "hello", "world", "hello", "world")]
        [InlineData(true, (long)10, 10.0d, 10.0d, 10.0d)]
        [InlineData(true, 10.0d, (long)10, 10.0d, 10.0d)]
        public void FixStateTypesShouldReturnCorrectValues(
            bool result, dynamic? newState, dynamic? oldState, dynamic? expectedNewState, dynamic? expectedOldState)
        {
            HassStateChangedEventData state = new HassStateChangedEventData
            {
                NewState = new HassState
                {
                    State = newState
                },
                OldState = new HassState
                {
                    State = oldState
                }
            };

            bool res = NetDaemonHost.FixStateTypes(state);
            Assert.Equal(result, res);
            Assert.Equal(expectedNewState, state.NewState.State);
            Assert.Equal(expectedOldState, state.OldState.State);
        }
    }
}