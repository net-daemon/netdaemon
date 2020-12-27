using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Fluent;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Storage;
using Xunit;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Base class for test classes
    /// </summary>
    public class DaemonHostTestBase : IAsyncLifetime
    {
        private Task? _fakeConnectedDaemon;
        private CancellationTokenSource? _cancelSource;

        /// <summary>
        ///     Default contructor
        /// </summary>
        public DaemonHostTestBase()
        {
            LoggerMock = new LoggerMock();
            DefaultHassClientMock = HassClientMock.DefaultMock;
            DefaultDataRepositoryMock = new Mock<IDataRepository>();

            DefaultHttpHandlerMock = new HttpHandlerMock();
            DefaultDaemonHost = new NetDaemonHost(
                DefaultHassClientMock.Object,
                DefaultDataRepositoryMock.Object,
                LoggerMock.LoggerFactory,
                DefaultHttpHandlerMock.Object)
            {
                InternalDelayTimeForTts = 0 // Allow no extra waittime
            };
        }

        /// <summary>
        ///     Returns default DaemonHost mock
        /// </summary>
        public NetDaemonHost DefaultDaemonHost { get; }
        /// <summary>
        ///     Returns default data repository mock
        /// </summary>
        public Mock<IDataRepository> DefaultDataRepositoryMock { get; }
        /// <summary>
        ///     Returns default HassClient mock
        /// </summary>
        public HassClientMock DefaultHassClientMock { get; }
        /// <summary>
        ///     Returns default HttpHandler mock
        /// </summary>
        public HttpHandlerMock DefaultHttpHandlerMock { get; }
        /// <summary>
        ///     Returns default logger mock
        /// </summary>
        public LoggerMock LoggerMock { get; }

        /// <summary>
        ///     Cleans up test
        /// </summary>
        public Task DisposeAsync()
        {
            _cancelSource?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Gets a object as dynamic
        /// </summary>
        /// <param name="testData">The object to turn into dynamic</param>
        public static dynamic GetDynamicDataObject(string testData = "testdata")
        {
            dynamic dynamicData = new ExpandoObject();
            dynamicData.Test = testData;
            return dynamicData;
        }

        /// <summary>
        ///     Converts parameters to dynamics
        /// </summary>
        public static (dynamic, FluentExpandoObject) GetDynamicObject(params (string, object)[] dynamicParameters)
        {
            var expandoObject = new FluentExpandoObject();
            var dict = expandoObject as IDictionary<string, object>;

            foreach (var (name, value) in dynamicParameters)
            {
                dict[name] = value;
            }
            return (expandoObject, expandoObject);
        }

        /// <summary>
        ///     Override for test init function
        /// </summary>
        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Sets fake current state of entity, adds it if entity not exists
        /// </summary>
        /// <param name="state">The state to sett</param>
        public void SetEntityState(HassState state)
        {
            _ = state ??
               throw new NetDaemonArgumentNullException(nameof(state));
            DefaultHassClientMock.FakeStates[state.EntityId] = state;
        }

        /// <summary>
        ///     Sets fake current state of entity, adds it if entity not exists
        /// </summary>
        /// <param name="entityId">Unique id of entity</param>
        /// <param name="state">State to set</param>
        /// <param name="area">Area of entity</param>
        public void SetEntityState(string entityId, dynamic? state = null, string? area = null)
        {
            DefaultDaemonHost.InternalState[entityId] = new EntityState
            {
                EntityId = entityId,
                Area = area,
                State = state
            };
        }

        /// <summary>
        ///     Adds an new instance of app
        /// </summary>
        /// <param name="app">The instance of the app to add</param>
        public async Task AddAppInstance(INetDaemonAppBase app)
        {
            _ = app ??
               throw new NetDaemonArgumentNullException(nameof(app));
            if (string.IsNullOrEmpty(app.Id))
                app.Id = Guid.NewGuid().ToString();
            DefaultDaemonHost.InternalAllAppInstances[app.Id] = app;
            DefaultDaemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
        }

        /// <summary>
        ///     Adds an simple state change event to NetDaemon to trigger apps
        /// </summary>
        /// <param name="entityId">Unique id of the entity</param>
        /// <param name="fromState">From state</param>
        /// <param name="toState">To state</param>
        public void AddChangedEvent(string entityId, object fromState, object toState)
        {
            DefaultHassClientMock.AddChangedEvent(entityId, fromState, toState);
        }

        /// <summary>
        ///     Adds a full home assistant fake event
        /// </summary>
        /// <param name="fromState"></param>
        /// <param name="toState"></param>
        public void AddChangeEvent(HassState? fromState, HassState? toState)
        {
            DefaultHassClientMock.AddChangeEventFull(fromState, toState);
        }

        /// <summary>
        ///     Adds an simple state change event to NetDaemon to trigger apps
        /// </summary>
        /// <param name="entityId">Unique id of the entity</param>
        /// <param name="fromState">From state</param>
        /// <param name="toState">To state</param>
        /// <param name="lastUpdated">Last updated time</param>
        /// <param name="lastChanged">Last changed time</param>
        public void AddChangedEvent(string entityId, object fromState, object toState, DateTime lastUpdated, DateTime lastChanged)
        {
            DefaultHassClientMock.AddChangedEvent(entityId, fromState, toState, lastUpdated, lastChanged);
        }

        /// <summary>
        ///     Add a fake event
        /// </summary>
        /// <param name="eventType">The id of the event</param>
        /// <param name="data">any custom data provided</param>
        public void AddCustomEvent(string eventType, dynamic? data)
        {
            DefaultHassClientMock.FakeEvents.Enqueue(new HassEvent
            {
                EventType = eventType,
                Data = data
            });
        }

        /// <summary>
        ///     Add a face service call event
        /// </summary>
        /// <param name="domain">Domain of event</param>
        /// <param name="service">Service to call</param>
        /// <param name="data">Custom data</param>
        public void AddCallServiceEvent(string domain, string service, dynamic? data = null)
        {
            DefaultHassClientMock.AddCallServiceEvent(domain, service, data);
        }

        /// <summary>
        ///     Verifies that a custom event are sent
        /// </summary>
        /// <param name="ev">Name of event</param>
        public void VerifyEventSent(string ev)
        {
            DefaultHassClientMock.Verify(n => n.SendEvent(ev, It.IsAny<object>()));
        }

        /// <summary>
        ///     Verifies that a custom event are sent
        /// </summary>
        /// <param name="ev">Name of event</param>
        /// <param name="eventData">Data sent by event</param>
        public void VerifyEventSent(string ev, object? eventData)
        {
            DefaultHassClientMock.Verify(n => n.SendEvent(ev, eventData));
        }

        /// <summary>
        ///    Verify that a service has been called
        /// </summary>
        /// <param name="domain">Domain of service</param>
        /// <param name="service">The service name</param>
        /// <param name="attributesTuples">Attributes</param>
        public void VerifyCallServiceTuple(string domain, string service,
            params (string attribute, object value)[] attributesTuples)
        {
            DefaultHassClientMock.VerifyCallServiceTuple(domain, service, attributesTuples);
        }

        /// <summary>
        ///     Verifies that call_service is called
        /// </summary>
        /// <param name="domain">Service domain</param>
        /// <param name="service">Service to verify</param>
        /// <param name="data">Data sent by service</param>
        /// <param name="waitForResponse">If service was waiting for response</param>
        /// <param name="times">Number of times called</param>
        public void VerifyCallService(string domain, string service, object? data = null, bool waitForResponse = false, Times? times = null)
        {
            DefaultHassClientMock.VerifyCallService(domain, service, data, waitForResponse, times);
        }

        /// <summary>
        ///     Verifies that call_service is called
        /// </summary>
        /// <param name="domain">Service domain</param>
        /// <param name="service">Service to verify</param>
        /// <param name="entityId">EntityId to verify</param>
        /// <param name="waitForResponse">If service was waiting for response</param>
        /// <param name="times">Number of times called</param>
        /// <param name="attributesTuples">Attributes to verify</param>
        public void VerifyCallService(string domain, string service, string entityId, bool waitForResponse = false, Times? times = null,
                params (string attribute, object value)[] attributesTuples)
        {
            var serviceObject = new FluentExpandoObject
            {
                ["entity_id"] = entityId
            };
            foreach (var (attr, val) in attributesTuples)
            {
                serviceObject[attr] = val;
            }

            DefaultHassClientMock.VerifyCallService(domain, service, serviceObject, waitForResponse, times);
        }

        /// <summary>
        ///     Verifies that call_service is called
        /// </summary>
        /// <param name="domain">Service domain</param>
        /// <param name="service">Service to verify</param>
        /// <param name="waitForResponse">If service was waiting for response</param>
        public void VerifyCallService(string domain, string service, bool waitForResponse = false)
        {
            DefaultHassClientMock.VerifyCallService(domain, service, waitForResponse);
        }

        /// <summary>
        ///     Verify that a service been called specific number of times
        /// </summary>
        /// <param name="service">Service name</param>
        /// <param name="times">Times called</param>
        public void VerifyCallServiceTimes(string service, Times times)
        {
            DefaultHassClientMock.VerifyCallServiceTimes(service, times);
        }

        /// <summary>
        ///     Wait for task until canceled
        /// </summary>
        /// <param name="task">Task to wait for</param>
        private static async Task WaitUntilCanceled(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }
        }

        /// <summary>
        ///     Initialize applications
        /// </summary>
        private async Task InitApps()
        {
            foreach (var inst in DefaultDaemonHost.InternalAllAppInstances)
            {
                await inst.Value.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
            }

            foreach (var inst in DefaultDaemonHost.InternalRunningAppInstances)
            {
                await inst.Value.InitializeAsync().ConfigureAwait(false);
                inst.Value.Initialize();
            }
        }

        /// <summary>
        ///     Verifies that a state is set
        /// </summary>
        /// <param name="entityId">Unique identifier of the entity</param>
        /// <param name="state">State being set</param>
        /// <param name="attributesTuples">Attributes being set</param>
        public void VerifySetState(string entityId, string state,
            params (string attribute, object value)[] attributesTuples)
        {
            DefaultHassClientMock.VerifySetState(entityId, state, attributesTuples);
        }

        /// <summary>
        ///     Verifies that state being set
        /// </summary>
        /// <param name="entityId">Unique identifier of the entity</param>
        /// <param name="times">How many times it being set</param>
        public void VerifySetStateTimes(string entityId, Times times)
        {
            DefaultHassClientMock.VerifySetStateTimes(entityId, times);
        }

        /// <summary>
        ///     Initialize the fake netdaemon core, must be run in most cases starting a test
        /// </summary>
        /// <param name="timeout">Timeout (ms) of how long fake daemon will stay connected and process events</param>
        /// <param name="overrideDebugNotCancel">True if running debug mode should not cancel on timeout</param>
        protected async Task InitializeFakeDaemon(short timeout = 300, bool overrideDebugNotCancel = false)
        {
            _fakeConnectedDaemon = await GetConnectedNetDaemonTask(timeout, overrideDebugNotCancel).ConfigureAwait(false);
        }

        /// <summary>
        ///     Runs the fake daemon service until timed out
        /// </summary>
        protected async Task RunFakeDaemonUntilTimeout()
        {
            _ = _fakeConnectedDaemon ??
                throw new NetDaemonNullReferenceException("No task to process, did you forget to run InitFakeDaemon at the beginning of the test?");

            await WaitUntilCanceled(_fakeConnectedDaemon).ConfigureAwait(false);
        }

        /// <summary>
        ///     Get already pre-connected mock NetDaemon object
        /// </summary>
        /// <param name="milliSeconds">Timeout in milliseconds</param>
        /// <param name="overrideDebugNotCancel">True to use timeout while debugging</param>
        private async Task<Task> GetConnectedNetDaemonTask(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            _cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                    ? new CancellationTokenSource()
                    : new CancellationTokenSource(milliSeconds);

            await InitApps().ConfigureAwait(false);

            var daemonTask = DefaultDaemonHost.Run("host", 8123, false, "token", _cancelSource.Token);
            await WaitForDefaultDaemonToConnect(DefaultDaemonHost, _cancelSource.Token).ConfigureAwait(false);
            return daemonTask;
        }

        private static async Task WaitForDefaultDaemonToConnect(NetDaemonHost daemonHost, CancellationToken cancellationToken)
        {
            var nrOfTimesCheckForConnectedState = 0;

            while (!daemonHost.Connected && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                if (nrOfTimesCheckForConnectedState++ > 100)
                    break;
            }
        }
    }
}