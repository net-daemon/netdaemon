using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Storage;
using Xunit;

namespace NetDaemon.Daemon.Fakes
{

    /// <summary>
    ///     Base class for test classes
    /// </summary>
    public partial class DaemonHostTestBase : IAsyncLifetime
    {
        private readonly NetDaemonHost _defaultDaemonHost;
        private readonly Mock<IDataRepository> _defaultDataRepositoryMock;
        private readonly HassClientMock _defaultHassClientMock;
        private readonly HttpHandlerMock _defaultHttpHandlerMock;
        private readonly LoggerMock _loggerMock;

        /// <summary>
        ///     Default contructor
        /// </summary>
        public DaemonHostTestBase()
        {
            _loggerMock = new LoggerMock();
            _defaultHassClientMock = HassClientMock.DefaultMock;
            _defaultDataRepositoryMock = new Mock<IDataRepository>();

            _defaultHttpHandlerMock = new HttpHandlerMock();
            _defaultDaemonHost = new NetDaemonHost(
                _defaultHassClientMock.Object,
                _defaultDataRepositoryMock.Object,
                _loggerMock.LoggerFactory,
                _defaultHttpHandlerMock.Object);

        }

        /// <summary>
        ///     Returns default DaemonHost mock
        /// </summary>
        public NetDaemonHost DefaultDaemonHost => _defaultDaemonHost;
        /// <summary>
        ///     Returns default data repository mock
        /// </summary>
        public Mock<IDataRepository> DefaultDataRepositoryMock => _defaultDataRepositoryMock;
        /// <summary>
        ///     Returns default HassClient mock
        /// </summary>
        public HassClientMock DefaultHassClientMock => _defaultHassClientMock;
        /// <summary>
        ///     Returns default HttpHandler mock
        /// </summary>
        public HttpHandlerMock DefaultHttpHandlerMock => _defaultHttpHandlerMock;
        /// <summary>
        ///     Returns default logger mock
        /// </summary>
        public LoggerMock LoggerMock => _loggerMock;

        Task IAsyncLifetime.DisposeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Gets a object as dynamic
        /// </summary>
        /// <param name="testData">The object to turn into dynamic</param>
        public dynamic GetDynamicDataObject(string testData = "testdata")
        {
            var expandoObject = new ExpandoObject();
            dynamic dynamicData = expandoObject;
            dynamicData.Test = testData;
            return dynamicData;
        }

        /// <summary>
        ///     Converts parameters to dynamics
        /// </summary>
        public (dynamic, ExpandoObject) GetDynamicObject(params (string, object)[] dynamicParameters)
        {
            var expandoObject = new ExpandoObject();
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
        ///     Adds an new instance of app
        /// </summary>
        /// <param name="app">The instance of the app to add</param>
        public void AddAppInstance(INetDaemonAppBase app)
        {
            if (string.IsNullOrEmpty(app.Id))
                throw new ArgumentException("Application needs an unique id, please provide it!");
            DefaultDaemonHost.InternalAllAppInstances[app.Id] = app;
            DefaultDaemonHost.InternalRunningAppInstances[app.Id] = app;
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
        /// <param name="hassEvent">Event to fake</param>
        public void AddChangedEvent(HassEvent hassEvent)
        {
            DefaultHassClientMock.FakeEvents.Enqueue(hassEvent);
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
        public void AddCallServiceEvent(string domain, string service, dynamic data)
        {
            DefaultHassClientMock.AddCallServiceEvent(domain, service, data);
        }

        /// <summary>
        ///    Verify that a service has been called
        /// </summary>
        /// <param name="domain">Domain of service</param>
        /// <param name="service">The service name</param>
        /// <param name="attributesTuples">Attributes</param>
        public void VerifyCallService(string domain, string service,
            params (string attribute, object value)[] attributesTuples)
        {
            DefaultHassClientMock.VerifyCallService(domain, service, attributesTuples);
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
        ///     Returns default DaemonHost
        /// </summary>
        /// <param name="milliSeconds">Timeout in ms</param>
        /// <param name="overrideDebugNotCancel">True if use timeout while debug test</param>
        public async Task<(Task, CancellationTokenSource)> ReturnRunningDefauldDaemonHostTask(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            await InitApps();
            var cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
            return (_defaultDaemonHost.Run("host", 8123, false, "token", cancelSource.Token), cancelSource);
        }


        /// <summary>
        ///     Runs the default daemon to process messages until canceled
        /// </summary>
        /// <param name="milliSeconds">Timeout time</param>
        /// <param name="overrideDebugNotCancel">True if debugging should cancel</param>
        /// <returns></returns>
        public async Task RunDefaultDaemonUntilCanceled(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            var cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
            try
            {
                await InitApps();
                await _defaultDaemonHost.Run("host", 8123, false, "token", cancelSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }
        }

        /// <summary>
        ///     Wait for task until canceled
        /// </summary>
        /// <param name="task">Task to wait for</param>
        /// <returns></returns>
        public async Task WaitUntilCanceled(Task task)
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
        public async Task InitApps()
        {
            foreach (var inst in DefaultDaemonHost.InternalAllAppInstances)
            {
                await inst.Value.StartUpAsync(_defaultDaemonHost);
            }

            foreach (var inst in DefaultDaemonHost.InternalRunningAppInstances)
            {
                await inst.Value.InitializeAsync();
                inst.Value.Initialize();
            }
        }

        /// <summary>
        ///     Get already pre-connected mock NetDaemon object
        /// </summary>
        /// <param name="milliSeconds">Timeout in milliseconds</param>
        /// <param name="overrideDebugNotCancel">True to use timeout while debugging</param>
        /// <returns></returns>
        protected async Task<Task> GetConnectedNetDaemonTask(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            var cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                    ? new CancellationTokenSource()
                    : new CancellationTokenSource(milliSeconds);

            await InitApps();

            var daemonTask = _defaultDaemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            await WaitForDefaultDaemonToConnect(DefaultDaemonHost, cancelSource.Token);
            return daemonTask;
        }

        /// <summary>
        ///     Wait until default NetDaemon has connected
        /// </summary>
        /// <param name="daemonHost">Daemon object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        protected async Task WaitForDefaultDaemonToConnect(NetDaemonHost daemonHost, CancellationToken cancellationToken)
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