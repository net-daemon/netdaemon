using System;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
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
    public class RxAppTest : CoreDaemonHostTestBase
    {
        [Fact]
        public async Task CallServiceShouldCallCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var (dynObj, _) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.CallService("mydomain", "myservice", dynObj);
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            VerifyCallServiceTuple("mydomain", "myservice", ("attr", "value"));
        }

        [Fact]
        public async Task NewAllEventDataShouldCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateAllChanges
                .Subscribe(_ => called = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task NewEventShouldCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.EventChanges
                .Subscribe(_ => called = true);

            DefaultHassClientMock.AddCustomEvent("AN_EVENT", new { somedata = "hello" });

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task NewEventMissingDataAttributeShouldReturnNull()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            string? missingAttribute = "has initial value";

            // ACT
            DefaultDaemonRxApp.EventChanges
                .Subscribe(s => missingAttribute = s.Data?.missing_data);

            dynamic dynExpObject = new ExpandoObject();
            dynExpObject.a_parameter = "hello";

            DefaultHassClientMock.AddCustomEvent("AN_EVENT", dynExpObject);

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Null(missingAttribute);
        }

        [Fact]
        public async Task NewStateEventShouldCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateChanges
                .Subscribe(_ => called = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task RunScriptShouldCallCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            // ACT
            DefaultDaemonRxApp.RunScript("myscript");
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT

            DefaultHassClientMock.VerifyCallServiceTimes("myscript", Times.Once());
        }

        [Fact]
        public async Task RunScriptWithDomainShouldCallCorrectFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            // ACT
            DefaultDaemonRxApp.RunScript("script.myscript");
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.VerifyCallServiceTimes("myscript", Times.Once());
        }

        [Fact]
        public async Task SameStateEventShouldNotCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.StateChanges
                .Subscribe(_ => called = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "on", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(called);
        }
        [Fact]
        public async Task SetStateShouldReturnCorrectData()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var (dynObj, expObj) = GetDynamicObject(
               ("attr", "value"));

            // ACT
            DefaultDaemonRxApp.SetState("sensor.any_sensor", "on", dynObj);
            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            DefaultHassClientMock.Verify(n => n.SetState("sensor.any_sensor", "on", expObj));
        }

        [Fact]
        public async Task StartupAsyncShouldThrowIfDaemonIsNull()
        {
            INetDaemonHost? host = null;

            // ARRANGE ACT ASSERT
            await Assert.ThrowsAsync<NetDaemonArgumentNullException>(() => DefaultDaemonRxApp.StartUpAsync(host!)).ConfigureAwait(false);
        }

        [Fact]
        public async Task StateShouldReturnCorrectEntity()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            // ACT
            var entity = DefaultDaemonRxApp.State("binary_sensor.pir");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(entity);
        }

        [Fact]
        public async Task StateShouldReturnNullIfAttributeNotExist()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);

            // ACT
            var entity = DefaultDaemonRxApp.State("binary_sensor.pir");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Null(entity?.Attribute?.not_exists);
        }

        [Fact]
        public async Task StatesShouldReturnCorrectEntity()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            var entity = DefaultDaemonRxApp.States.FirstOrDefault(n => n.EntityId == "binary_sensor.pir");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);
            // ASSERT
            Assert.NotNull(entity);
            Assert.Equal("binary_sensor.pir", entity?.EntityId);
        }

        [Fact]
        public async Task EntityIdsShouldReturnCorrectItems()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT
            DefaultDaemonHost.InternalState.Clear();
            DefaultDaemonHost.InternalState["light.mylight"] = new();
            DefaultDaemonHost.InternalState["light.mylight2"] = new();
            var entities = DefaultDaemonRxApp.EntityIds.ToList();

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);
            // ASSERT
            Assert.NotNull(entities);
            Assert.Equal(2, entities.Count);
        }

        [Fact]
        public async Task UsingEntitiesLambdaNewEventShouldCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entities(n => n.EntityId.StartsWith("binary_sensor.pir", true, CultureInfo.InvariantCulture))
                .StateChanges
                .Subscribe(_ => called = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task CallbackObserverAttributeMissingShouldReturnNull()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            string? missingString = "has initial value";

            // ACT
            DefaultDaemonRxApp.Entities(n => n.EntityId.StartsWith("binary_sensor.pir", true, CultureInfo.InvariantCulture))
                .StateChanges
                .Subscribe(s => missingString = s.New.Attribute?.missing_attribute);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Null(missingString);
        }

        [Fact]
        public async Task UsingEntitiesNewEventShouldCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entities("binary_sensor.pir", "binary_sensor.pir_2")
                .StateChanges
                .Subscribe(_ => called = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir_2", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntityNewEventShouldCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => called = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.True(called);
        }

        [Fact]
        public async Task UsingEntityNewEventShouldNotCallFunction()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            var called = false;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.other_pir")
                .StateChanges
                .Subscribe(_ => called = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.False(called);
        }
        [Fact]
        public async Task WhenStateStaysSameForTimeItShouldCallFunction()
        {
            await InitializeFakeDaemon(500).ConfigureAwait(false);

            bool isRun = false;
            using var ctx = DefaultDaemonRxApp.StateChanges
                .Where(t => t.New.EntityId == "binary_sensor.pir")
                .NDSameStateFor(TimeSpan.FromMilliseconds(50))
                .Subscribe(_ => isRun = true);

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            Assert.True(isRun);
        }

        [Fact]
        public async Task SavedDataShouldReturnSameDataUsingExpando()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            dynamic data = new ExpandoObject();
            data.Item = "Some data";

            // ACT
            DefaultDaemonRxApp.SaveData("data_exists", data);
            var collectedData = DefaultDaemonRxApp.GetData<ExpandoObject>("data_exists");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Equal(data, collectedData);
        }

        [Fact]
        public async Task GetDataShouldReturnCachedValue()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            // ACT

            DefaultDaemonRxApp.SaveData("GetDataShouldReturnCachedValue_id", "saved data");

            DefaultDaemonRxApp.GetData<string>("GetDataShouldReturnCachedValue_id");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            DefaultDataRepositoryMock.Verify(n => n.Get<string>(It.IsAny<string>()), Times.Never);
            DefaultDataRepositoryMock.Verify(n => n.Save(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void DelayShouldDelaySyncronyslyWithToken()
        {
            // ARRANGE
            var startTime = DateTime.Now;
            using var tokenSource = new CancellationTokenSource();
            // ACT
            DefaultDaemonRxApp.Delay(TimeSpan.FromMilliseconds(100), tokenSource.Token);
            // ASSERT
            // Compensate that windows resolution is 15ms for system clock
            bool isAfterTimeout = DateTime.Now.Subtract(startTime).TotalMilliseconds >= 84;
            Assert.True(isAfterTimeout);
        }

        [Fact]
        public void DelayShouldDelaySyncronysly()
        {
            // ARRANGE
            var startTime = DateTime.Now;
            // ACT
            DefaultDaemonRxApp.Delay(TimeSpan.FromMilliseconds(100));
            // Compensate that windows resolution is 15ms for system clock
            bool isAfterTimeout = DateTime.Now.Subtract(startTime).TotalMilliseconds >= 84;
            // ASSERT
            Assert.True(isAfterTimeout);
        }

        [Fact]
        public async Task NDFirstOrTimeOutShouldReturnCorrectStateChange()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            (EntityState Old, EntityState New)? result = null;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => result = DefaultDaemonRxApp.Entity("binary_sensor.pir2").StateChanges.NDFirstOrTimeout(TimeSpan.FromMilliseconds(200)));

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");
            await Task.Delay(100).ConfigureAwait(false);
            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir2", "on", "off");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async Task NDFirstOrTimeOutShouldReturnCorrectNullOnTimeout()
        {
            // ARRANGE
            await InitializeFakeDaemon().ConfigureAwait(false);
            (EntityState Old, EntityState New)? result = null;

            // ACT
            DefaultDaemonRxApp.Entity("binary_sensor.pir")
                .StateChanges
                .Subscribe(_ => result = DefaultDaemonRxApp.Entity("binary_sensor.pir2").StateChanges.NDFirstOrTimeout(TimeSpan.FromMilliseconds(100)));

            DefaultHassClientMock.AddChangedEvent("binary_sensor.pir", "off", "on");

            await RunFakeDaemonUntilTimeout().ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void DelayShouldCancelWithToken()
        {
            // ARRANGE
            using var tokenSource = new CancellationTokenSource(50);
            // ACT & ASSERT
            Assert.Throws<OperationCanceledException>(() => DefaultDaemonRxApp.Delay(TimeSpan.FromMilliseconds(300), tokenSource.Token));
        }

        private interface ITestGetService
        {
            string TestString { get; }
        }
        private class TestGetService : ITestGetService
        {
            public string TestString => "Test";
        }

        [Fact]
        public void ServiceProviderShouldReturnCorrectService()
        {
            // ARRANGE
            DefaultServiceProviderMock.Services[typeof(ITestGetService)] = new TestGetService();
            // ACT
            var service = DefaultDaemonRxApp.ServiceProvider?.GetService(typeof(ITestGetService)) as TestGetService;
            // ASSERT
            Assert.NotNull(service);
            Assert.Equal("Test", service?.TestString);
        }
    }
}