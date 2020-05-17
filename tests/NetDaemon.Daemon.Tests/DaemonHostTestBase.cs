using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage;
using Moq;
using NetDaemon.Daemon.Tests.Daemon;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class BaseTestApp : JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp { }

    public class BaseTestRxApp : JoySoftware.HomeAssistant.NetDaemon.Common.Reactive.NetDaemonRxApp { }

    public partial class DaemonHostTestBase : IAsyncLifetime
    {
        private readonly JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp _defaultDaemonApp;
        private readonly NetDaemonHost _defaultDaemonHost;
        private readonly BaseTestRxApp _defaultDaemonRxApp;
        private readonly Mock<IDataRepository> _defaultDataRepositoryMock;
        private readonly HassClientMock _defaultHassClientMock;
        private readonly HttpHandlerMock _defaultHttpHandlerMock;
        private readonly LoggerMock _loggerMock;
        private readonly NetDaemonHost _notConnectedDaemonHost;

        internal DaemonHostTestBase()
        {
            _loggerMock = new LoggerMock();
            _defaultHassClientMock = HassClientMock.DefaultMock;
            _defaultDataRepositoryMock = new Mock<IDataRepository>();

            _defaultHttpHandlerMock = new HttpHandlerMock();
            _defaultDaemonHost = new NetDaemonHost(
                new Mock<IInstanceDaemonApp>().Object,
                _defaultHassClientMock.Object,
                _defaultDataRepositoryMock.Object,
                _loggerMock.LoggerFactory,
                _defaultHttpHandlerMock.Object);

            _defaultDaemonApp = new BaseTestApp();
            _defaultDaemonApp.Id = "app_id";

            _defaultDaemonHost.InternalRunningAppInstances[_defaultDaemonApp.Id!] = _defaultDaemonApp;

            _defaultDaemonRxApp = new BaseTestRxApp();
            _defaultDaemonRxApp.Id = "app_rx_id";
            _defaultDaemonHost.InternalRunningAppInstances[_defaultDaemonRxApp.Id!] = _defaultDaemonRxApp;

            _notConnectedDaemonHost = new NetDaemonHost(new Mock<IInstanceDaemonApp>().Object, HassClientMock.MockConnectFalse.Object, _defaultDataRepositoryMock.Object, _loggerMock.LoggerFactory);
        }

        public JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp DefaultDaemonApp => _defaultDaemonApp;

        public NetDaemonHost DefaultDaemonHost => _defaultDaemonHost;

        public BaseTestRxApp DefaultDaemonRxApp => _defaultDaemonRxApp;

        public Mock<IDataRepository> DefaultDataRepositoryMock => _defaultDataRepositoryMock;

        public HassClientMock DefaultHassClientMock => _defaultHassClientMock;

        public HttpHandlerMock DefaultHttpHandlerMock => _defaultHttpHandlerMock;

        public string HelloWorldData => "Hello world!";

        public LoggerMock LoggerMock => _loggerMock;

        public NetDaemonHost NotConnectedDaemonHost => _notConnectedDaemonHost;

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _defaultDaemonApp.DisposeAsync().ConfigureAwait(false);
            await _defaultDaemonRxApp.DisposeAsync().ConfigureAwait(false);
            await _defaultDaemonHost.DisposeAsync().ConfigureAwait(false);
        }

        public dynamic GetDynamicDataObject(string testData = "testdata")
        {
            var expandoObject = new ExpandoObject();
            dynamic dynamicData = expandoObject;
            dynamicData.Test = testData;
            return dynamicData;
        }

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

        public async Task InitializeAsync()
        {
            await _defaultDaemonApp.StartUpAsync(_defaultDaemonHost).ConfigureAwait(false);
            await _defaultDaemonRxApp.StartUpAsync(_defaultDaemonHost).ConfigureAwait(false);
        }

        public (Task, CancellationTokenSource) ReturnRunningDefauldDaemonHostTask(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            var cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
            return (_defaultDaemonHost.Run("host", 8123, false, "token", cancelSource.Token), cancelSource);
        }

        public (Task, CancellationTokenSource) ReturnRunningNotConnectedDaemonHostTask(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            var cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
            return (_notConnectedDaemonHost.Run("host", 8123, false, "token", cancelSource.Token), cancelSource);
        }

        public async Task RunDefauldDaemonUntilCanceled(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            var cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
            try
            {
                await _defaultDaemonHost.Run("host", 8123, false, "token", cancelSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }
        }

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

        protected async Task WaitForDefaultDaemonToConnect(NetDaemonHost daemonHost, CancellationToken stoppingToken)
        {
            var nrOfTimesCheckForConnectedState = 0;

            while (!daemonHost.Connected && !stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(50, stoppingToken).ConfigureAwait(false);
                if (nrOfTimesCheckForConnectedState++ > 5)
                    break;
            }
        }
    }
}