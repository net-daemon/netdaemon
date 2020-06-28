using System;
using System.Dynamic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Moq;
using NetDaemon.Common.Reactive;
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
    public class RxAppTest : DaemonHostTestBase
    {
        public RxAppTest() : base()
        {
        }

        [Fact]
        public async Task CallServiceShouldCallCorrectFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.CallService("mydomain", "myservice", dynObj);
            await daemonTask;

            // ASSERT
            DefaultHassClientMock.VerifyCallService("mydomain", "myservice", ("attr", "value"));
        }

        [Fact]
        public async Task NewAllEventDataShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateAllChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task NewEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.EventChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddCustomEvent("AN_EVENT", new { somedata = "hello" });

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task NewEventMissingDataAttributeShouldReturnNull()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
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

            await daemonTask;

            // ASSERT
            Assert.Null(missingAttribute);
        }

        [Fact]
        public async Task NewStateEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task RunScriptShouldCallCorrectFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.RunScript("myscript");
            await daemonTask;

            // ASSERT


            DefaultHassClientMock.VerifyCallServiceTimes("myscript", Times.Once());
        }

        [Fact]
        public async Task RunScriptWithDomainShouldCallCorrectFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.RunScript("script.myscript");
            await daemonTask;

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("myscript", Times.Once());
        }

        [Fact]
        public async Task SameStateEventShouldNotCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "on");

            await daemonTask;

            // ASSERT
            Assert.False(called);
        }
        [Fact]
        public async Task SetStateShouldReturnCorrectData()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.SetState("sensor.any_sensor", "on", dynObj);
            await daemonTask;

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
            var daemonTask = await GetConnectedNetDaemonTask();

            // ACT
            var entity = DefaultDaemonRxApp.State("binary_sensor.pir");

            await daemonTask;

            // ASSERT
            Assert.NotNull(entity);
        }

        [Fact]
        public async Task StateShouldReturnNullIfAttributeNotExist()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();

            // ACT
            var entity = DefaultDaemonRxApp.State("binary_sensor.pir");

            await daemonTask;

            // ASSERT
            Assert.Null(entity?.Attribute?.not_exists);
        }

        [Fact]
        public async Task StatesShouldReturnCorrectEntity()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            // ACT
            var entity = DefaultDaemonRxApp.States.FirstOrDefault(n => n.EntityId == "binary_sensor.pir");

            await daemonTask.ConfigureAwait(false);
            // ASSERT
            Assert.NotNull(entity);
            Assert.Equal("binary_sensor.pir", entity.EntityId);
        }

        [Fact]
        public async Task UsingEntitiesLambdaNewEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entities(n => n.EntityId.StartsWith("binary_sensor.pir"))
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task CallbackObserverAttributeMissingShouldReturnNull()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            string? missingString = "has initial value";

            // ACT
            DefaultDaemonRxApp.Entities(n => n.EntityId.StartsWith("binary_sensor.pir"))
                .StateChanges
                .Subscribe(s =>
                {
                    missingString = s.New.Attribute?.missing_attribute;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.Null(missingString);
        }

        [Fact]
        public async Task UsingEntitiesNewEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entities("binary_sensor.pir", "binary_sensor.pir_2")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntityNewEventShouldCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntityNewEventShouldNotCallFunction()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.other_pir")
                .StateChanges
                .Subscribe(s =>
                {
                    called = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await daemonTask;

            // ASSERT
            Assert.False(called);
        }
        [Fact]
        public async Task WhenStateStaysSameForTimeItShouldCallFunction()
        {
            var daemonTask = await GetConnectedNetDaemonTask(300);

            bool isRun = false;
            using var ctx = DefaultDaemonRxApp.StateChanges
                .Where(t => t.New.EntityId == "binary_sensor.pir")
                .NDSameStateFor(TimeSpan.FromMilliseconds(50))
                .Subscribe(e =>
                {
                    isRun = true;
                });

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await daemonTask;

            Assert.True(isRun);
        }

        [Fact]
        public async Task SavedDataShouldReturnSameDataUsingExpando()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            dynamic data = new ExpandoObject();
            data.Item = "Some data";

            // ACT
            DefaultDaemonRxApp.SaveData("data_exists", data);
            var collectedData = DefaultDaemonRxApp.GetData<ExpandoObject>("data_exists");

            await daemonTask;

            // ASSERT
            Assert.Equal(data, collectedData);
        }

        [Fact]
        public async Task GetDataShouldReturnCachedValue()
        {
            // ARRANGE
            var daemonTask = await GetConnectedNetDaemonTask();
            // ACT

            DefaultDaemonRxApp.SaveData("GetDataShouldReturnCachedValue_id", "saved data");

            DefaultDaemonRxApp.GetData<string>("GetDataShouldReturnCachedValue_id");

            await daemonTask;
            // ASSERT
            DefaultDataRepositoryMock.Verify(n => n.Get<string>(It.IsAny<string>()), Times.Never);
            DefaultDataRepositoryMock.Verify(n => n.Save<string>(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}