using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Dynamic;
using System.Threading.Tasks;
using NetDaemon.Common;
using Xunit;
using NetDaemon.Daemon.Fakes;
using NetDaemon.Daemon.Tests.DaemonRunner.App;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Daemon.Tests.Daemon
{
    public class HostTestApp : NetDaemonRxApp
    {
    }

    public class NetDaemonTests : CoreDaemonHostTestBase
    {
        public NetDaemonTests() : base()
        {
        }

        [Fact]
        public async Task AttributeServiceCallShouldFindCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            await using var app = new AssemblyDaemonApp() { Id = "id" };
            DefaultDaemonHost.AddRunningApp(app);

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

        [Fact]
        public async Task RunNotConnectedCompletesTask()
        {
            // ARRANGE

            // ACTION
            var (runTask, _) = ReturnRunningNotConnectedDaemonHostTask();
            await runTask.ConfigureAwait(false);

            // ASSERT
            Assert.True(runTask.IsCompleted);
        }

        [Fact]
        public void RunNullReferenceToHassClientShouldThrowException()
        {
            // ARRANGE

            // ACT and ASSERT
            Assert.Throws<ArgumentNullException>(() =>
            {
                var DefaultDaemonHost = new NetDaemonHost(null, null);
            });
        }

        [Fact]
        public async Task SendEventShouldCallCorrectMethod()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var eventData = DaemonHostTestBase.GetDynamicDataObject();

            // ACT
            await DefaultDaemonHost.SendEvent("test_event", eventData).ConfigureAwait(false);

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
            await DefaultDaemonHost.SendEvent("test_event").ConfigureAwait(false);
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

            DefaultDaemonHost.TextToSpeechService.InternalDelayTimeForTts = 100; // For testing

            // ACT
            DefaultDaemonHost.Speak("media_player.fakeplayer", "Hello test!");

            var (_, expObject) = GetDynamicObject(
                ("entity_id", "media_player.fakeplayer"),
                ("message", "Hello test!")
            );

            await DefaultDaemonHost.WaitForTasksAsync().ConfigureAwait(false);

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
                Attributes = new Dictionary<string, object>()
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
            var (_, expectedAttributesExpObject) = GetDynamicObject(
                ("entity_id", "media_player.fakeplayer"),
                ("message", "Hello test!")
            );

            // ACT
            DefaultDaemonHost.Speak("media_player.fakeplayer", "Hello test!");
            DefaultDaemonHost.Speak("media_player.fakeplayer", "Hello test!");

            // ASSERT

            await Task.Delay(50).ConfigureAwait(false);

            VerifyCallService("tts", "google_cloud_say", expectedAttributesExpObject, true, Times.Once());

            await DefaultDaemonHost.WaitForTasksAsync().ConfigureAwait(false);
            // Called twice
            VerifyCallService("tts", "google_cloud_say", expectedAttributesExpObject, true, Times.Exactly(2));
        }

        [Fact]
        public async Task StopCallsCloseClient()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            await DefaultDaemonHost.Stop().ConfigureAwait(false);

            DefaultHassClientMock.Verify(n => n.CloseAsync(), Times.Once);
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
        public async Task CallServiceEventShouldCallCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var dynObject = DaemonHostTestBase.GetDynamicDataObject(HelloWorldData);

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
        [SuppressMessage("", "CA1508")]
        public async Task CallServiceEventOtherShouldNotCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var dynObject = DaemonHostTestBase.GetDynamicDataObject(HelloWorldData);

            DefaultHassClientMock.AddCallServiceEvent("custom_domain", "other_service", dynObject);

            var isCalled = false;
            string? message = null;

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
            await InitializeFakeDaemon().ConfigureAwait(false);
            await DefaultDaemonHost.SetStateAsync("sensor.any_sensor", "on", ("attr", "value")).ConfigureAwait(false);

            var (dynObj, expObj) = GetDynamicObject(
                ("attr", "value")
            );
            DefaultHassClientMock.Verify(n => n.SetState("sensor.any_sensor", "on", expObj));
        }

        [Fact]
        public async Task SetStateWaitForResultShouldCallCorrectFunction()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            var (dynObj, expObj) = GetDynamicObject(
                ("attr", "value")
            );
            var entity = await DefaultDaemonHost
                .SetStateAndWaitForResponseAsync("sensor.any_sensor", "on", dynObj, true)
                .ConfigureAwait(false);

            DefaultHassClientMock.Verify(n => n.SetState("sensor.any_sensor", "on", expObj));
        }

        [Fact]
        public async Task SetStateDynamicNullReturnOfGetStateShouldCallCorrectFunction()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            var (dynObj, expObj) = GetDynamicObject(
                ("attr", "value")
            );
            DefaultDaemonHost.HasNetDaemonIntegration = true;
            DefaultHassClientMock.Setup(n => n.GetState(It.IsAny<string>())).Returns(Task.FromResult<HassState?>(null));
            var entity = await DefaultDaemonHost
                .SetStateAndWaitForResponseAsync("sensor.any_sensor", "on", new { attr = "value" }, true)
                .ConfigureAwait(false);

            DefaultHassClientMock.Verify(n => n.CallService("netdaemon", "entity_create",
                It.IsAny<object>(), null, true), Times.Once);

            DefaultHassClientMock.Verify(n => n.GetState("sensor.any_sensor"));
            Assert.Null(entity);
        }

        [Fact]
        public async Task SetStateDynamicShouldCallCorrectFunction()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            var (dynObj, expObj) = GetDynamicObject(
                ("attr", "value")
            );
            DefaultDaemonHost.HasNetDaemonIntegration = true;
            DefaultHassClientMock.Setup(n => n.GetState(It.IsAny<string>()))
                .Returns(Task.FromResult<HassState?>(new HassState()));
            var entity = await DefaultDaemonHost
                .SetStateAndWaitForResponseAsync("sensor.any_sensor", "on", new { attr = "value" }, true)
                .ConfigureAwait(false);

            DefaultHassClientMock.Verify(n => n.CallService("netdaemon", "entity_create",
                It.IsAny<object>(), null, true), Times.Once);

            DefaultHassClientMock.Verify(n => n.GetState("sensor.any_sensor"));
            Assert.NotNull(entity);
        }

        [Fact]
        public async Task SetStateDynamicWithNoWaitShouldCallCorrectFunction()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            var (dynObj, expObj) = GetDynamicObject(
                ("attr", "value")
            );
            DefaultDaemonHost.HasNetDaemonIntegration = true;
            var entity = await DefaultDaemonHost
                .SetStateAndWaitForResponseAsync("sensor.any_sensor", "on", new { attr = "value" }, false)
                .ConfigureAwait(false);

            DefaultHassClientMock.Verify(n => n.CallService("netdaemon", "entity_create",
                It.IsAny<object>(), null, false), Times.Once);

            DefaultHassClientMock.Verify(n => n.GetState("sensor.any_sensor"), Times.Never);
            Assert.Null(entity);
        }

        [Fact]
        public async Task SetStateShouldReturnCorrectData()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            await DefaultDaemonHost.SetStateAsync("sensor.any_sensor", "on", ("attr", "value")).ConfigureAwait(false);

            var (dynObj, expObj) = GetDynamicObject(
                ("attr", "value")
            );
            DefaultHassClientMock.Verify(n => n.SetState("sensor.any_sensor", "on", expObj));
        }

        [Fact]
        public async Task GetServicesShouldCallCorrectFunctions()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            var services = await DefaultDaemonHost.GetAllServices().ConfigureAwait(false);

            DefaultHassClientMock.Verify(n => n.GetServices(), Times.Once);
        }

        [Fact]
        public async Task SetDaemonStateAsyncShouldCallCorrectFunctions()
        {
            await InitializeFakeDaemon().ConfigureAwait(false);
            await DefaultDaemonHost.SetDaemonStateAsync(5, 2).ConfigureAwait(false);

            DefaultHassClientMock.Verify(n => n.SetState("sensor.netdaemon_status", "Connected", It.IsAny<object>()),
                Times.Once);
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

        private interface ITestGetService
        {
            string TestString { get; }
        }

        private class TestGetService : ITestGetService
        {
            public string TestString => "Test";
        }

        [SuppressMessage("", checkId:"CA1034")]
        public class ServiceProviderTest : DaemonHostTestBase
        {
            // This test is in a nested class because the CoreDaemonHostTestBase will create the ServiceProvider before
            // we get a chance to add additional services to the ServiceCollection.  DaemonHostTestBase doe snot do that
            [Fact]
            public void ServiceProviderShouldReturnCorrectService()
            {
                // ARRANGE
                DefaultServiceCollection.AddSingleton<ITestGetService>(new TestGetService());
                // ACT
                var service = DefaultDaemonHost.ServiceProvider?.GetService(typeof(ITestGetService)) as TestGetService;
                // ASSERT
                Assert.NotNull(service);
                Assert.Equal("Test", service?.TestString);
            }
        }

        [Fact]
        public void EntityShouldReturnCorrectValueForArea()
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
        public void EntityShouldReturnCorrectValueForAreaAssignedToEntity()
        {
            // ARRANGE
            DefaultDaemonHost._hassEntities["light.lamp"] = new HassEntity
            {
                EntityId = "light.lamp",
                AreaId = "some area"
            };
            // ACT
            var areaName = DefaultDaemonHost.GetAreaForEntityId("light.lamp");

            // ASSERT
            Assert.Equal("some area", areaName);
        }

        [Fact]
        public void EntityShouldReturnNullForAreaNotExist()
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
            Assert.Equal("Correct name", DefaultDaemonHost.GetState("binary_sensor.pir")?.Area);
        }

        [Fact]
        public async Task SetStateShouldKeepSameArea()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            var state = await DefaultDaemonHost.SetStateAsync("light.light_in_area", "on", ("attr", "value"))
                .ConfigureAwait(false);
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);
            /// ASSERT
            Assert.Equal("Area", state?.Area);
        }

        [Fact]
        public async Task ConnectToHAIntegrationShouldCallCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            await DefaultDaemonHost.ConnectToHAIntegration().ConfigureAwait(false);

            // ACT
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.Verify(n => n.GetApiCall<object>("netdaemon/info"));
        }

        [Theory]
        [InlineData(false, null, null, null, null)]
        [InlineData(true, null, 10L, null, 10L)]
        [InlineData(true, 10L, null, 10L, null)]
        [InlineData(true, "unavailable", 10L, null, 10L)]
        [InlineData(true, 10L, "unavailable", 10L, null)]
        [InlineData(true, 10L, 11L, 10L, 11L)]
        [InlineData(true, "hello", "world", "hello", "world")]
        [InlineData(true, 10L, 10.1d, 10.0d, 10.1d)]
        [InlineData(true, 10.1d, 10L, 10.1d, 10.0d)]
        public void FixStateTypesShouldReturnCorrectValues(
            bool result, object? newState, object? oldState, object? expectedNewState, object? expectedOldState)
        {
            var newEntityState = new EntityState
            {
                State = newState
            };
            var oldEntityState = new EntityState
            {
                State = oldState
            };

            bool res = NetDaemonHost.FixStateTypes(ref oldEntityState, ref newEntityState);

            Assert.Equal(result, res);
            Assert.Equal(expectedNewState, newEntityState.State);
            Assert.Equal(expectedOldState, oldEntityState.State);
        }
    }
}