using System;
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using Moq;
using NetDaemon.Common.Fluent;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Fakes;
using Xunit;

namespace NetDaemon.Daemon.Tests.Reactive
{
    /// <summary>
    ///     Tests the fluent API parts of the daemon
    /// </summary>
    /// <remarks>
    ///     Mainly the tests checks if correct underlying call to "CallService"
    ///     has been made.
    /// </remarks>
    public class FakeTests : CoreDaemonHostTestBase
    {
        public FakeTests() : base()
        {
        }

        [Fact]
        public async Task CallServiceShouldCallCorrectFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.CallService("mydomain", "myservice", dynObj);
            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            VerifyCallServiceTuple("mydomain", "myservice", ("attr", "value"));
        }

        [Fact]
        public async Task NewAllEventDataShouldCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateAllChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task NewEventShouldCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.EventChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddCustomEvent("AN_EVENT", new { somedata = "hello" });

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task NewEventMissingDataAttributeShouldReturnNull()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            string? missingAttribute = "has initial value";

            // ACT
            DefaultDaemonRxApp.EventChanges
                .Subscribe(s =>
                {
                    missingAttribute = s.Data?.missing_data;
                });

            var expandoObj = new ExpandoObject();
            dynamic dynExpObject = expandoObj;
            dynExpObject.a_parameter = "hello";

            DefaultHassClientMock.AddCustomEvent("AN_EVENT", dynExpObject);

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Null(missingAttribute);
        }

        [Fact]
        public async Task NewStateEventShouldCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task RunScriptShouldCallCorrectFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.RunScript("myscript");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT


            DefaultHassClientMock.VerifyCallServiceTimes("myscript", Times.Once());
        }

        [Fact]
        public async Task RunScriptWithDomainShouldCallCorrectFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.RunScript("script.myscript");
            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("myscript", Times.Once());
        }

        [Fact]
        public async Task SameStateEventShouldNotCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(called);
        }
        [Fact]
        public async Task SetStateShouldReturnCorrectData()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.SetState("sensor.any_sensor", "on", dynObj);
            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.Verify(n => n.SetState("sensor.any_sensor", "on", expObj));
        }

        [Fact]
        public async Task StartupAsyncShouldThrowIfDaemonIsNull()
        {
            INetDaemonHost? host = null;

            // ARRANGE ACT ASSERT
            await Assert.ThrowsAsync<NullReferenceException>(() => DefaultDaemonRxApp.StartUpAsync(host!));
        }

        [Fact]
        public async Task StateShouldReturnCorrectEntity()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            // ACT
            var entity = DefaultDaemonRxApp.State("binary_sensor.pir");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(entity);
        }

        [Fact]
        public async Task StateShouldReturnNullIfAttributeNotExist()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            // ACT
            var entity = DefaultDaemonRxApp.State("binary_sensor.pir");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Null(entity?.Attribute?.not_exists);
        }

        [Fact]
        public async Task StatesShouldReturnCorrectEntity()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            // ACT
            var entity = DefaultDaemonRxApp.States.FirstOrDefault(n => n.EntityId == "binary_sensor.pir");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            // ASSERT
            Assert.NotNull(entity);
            Assert.Equal("binary_sensor.pir", entity?.EntityId);
        }

        [Fact]
        public async Task EntityIdsShouldReturnCorrectItems()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            // ACT
            var entities = DefaultDaemonRxApp.EntityIds.ToList();

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            // ASSERT
            Assert.NotNull(entities);
            Assert.Equal(8, entities.Count());
        }

        [Fact]
        public async Task UsingEntitiesLambdaNewEventShouldCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entities(n => n.EntityId.StartsWith("binary_sensor.pir"))
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task CallbackObserverAttributeMissingShouldReturnNull()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);

            string? missingString = "has initial value";

            // ACT
            DefaultDaemonRxApp.Entities(n => n.EntityId.StartsWith("binary_sensor.pir"))
                .StateChanges
                .Subscribe(s =>
                {
                    missingString = s.New.Attribute?.missing_attribute;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Null(missingString);
        }

        [Fact]
        public async Task UsingEntitiesNewEventShouldCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entities("binary_sensor.pir", "binary_sensor.pir_2")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntityNewEventShouldCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntityNewEventShouldNotCallFunction()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.other_pir")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(called);
        }
        [Fact]
        public async Task WhenStateStaysSameForTimeItShouldCallFunction()
        {
            await FakeDaemonInit().ConfigureAwait(false);

            bool isRun = false;
            using var ctx = DefaultDaemonRxApp.StateChanges
                .Where(t => t.New.EntityId == "binary_sensor.pir")
                .NDSameStateFor(TimeSpan.FromMilliseconds(50))
                .Subscribe(e =>
                {
                    isRun = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            Assert.True(isRun);
        }

        [Fact]
        public async Task SavedDataShouldReturnSameDataUsingExpando()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            dynamic data = new ExpandoObject();
            data.Item = "Some data";

            // ACT
            DefaultDaemonRxApp.SaveData("data_exists", data);
            var collectedData = DefaultDaemonRxApp.GetData<ExpandoObject>("data_exists");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Equal(data, collectedData);
        }

        [Fact]
        public async Task GetDataShouldReturnCachedValue()
        {
            // ARRANGE
            await FakeDaemonInit().ConfigureAwait(false);
            // ACT

            DefaultDaemonRxApp.SaveData("GetDataShouldReturnCachedValue_id", "saved data");

            DefaultDaemonRxApp.GetData<string>("GetDataShouldReturnCachedValue_id");

            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);
            // ASSERT
            DefaultDataRepositoryMock.Verify(n => n.Get<string>(It.IsAny<string>()), Times.Never);
            DefaultDataRepositoryMock.Verify(n => n.Save<string>(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TestFakeAppTurnOnCorrectLight()
        {
            // Add the app to test
            await AddAppInstance(new FakeApp());

            // Init NetDaemon core runtime
            await FakeDaemonInit().ConfigureAwait(false);

            // Fake a changed event from en entity
            AddChangedEvent("binary_sensor.kitchen", "off", "on");

            // Run the NetDemon Core to process the messages
            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // Verify that netdaemon called light.turn_on 
            VerifyCallService("light", "turn_on", "light.kitchen");
        }


        [Fact]
        public async Task TestFakeAppCallNoteWhenBatteryLevelBelowValue()
        {
            // Add the app to test
            await AddAppInstance(new FakeApp());

            // Init NetDaemon core runtime
            await FakeDaemonInit().ConfigureAwait(false);

            // Fake a changed event from en entity
            AddChangeEvent(new()
            {
                EntityId = "sensor.temperature",
                State = 10.0,
                Attributes = new()
                {
                    ["battery_level"] = 18.2
                }
            }
            , new()
            {
                EntityId = "sensor.temperature",
                State = 10.0,
                Attributes = new()
                {
                    ["battery_level"] = 12.0
                }
            });

            // Run the NetDemon Core to process the messages
            await FakeRunDaemonCoreUntilTimeout().ConfigureAwait(false);

            // Verify that netdaemon called light.turn_on 
            VerifyCallService("notify", "notify", new { title = "Hello from Home Assistant" });
        }
    }
}