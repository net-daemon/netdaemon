using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage;
using Moq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

namespace NetDaemon.Daemon.Tests
{
    public class BaseTestApp : JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp { }

    public partial class DaemonHostTestBase
    {
        private readonly LoggerMock _loggerMock;
        private readonly HassClientMock _defaultHassClientMock;
        private readonly Mock<IDataRepository> _defaultDataRepositoryMock;
        private readonly NetDaemonHost _defaultDaemonHost;
        private readonly NetDaemonHost _notConnectedDaemonHost;

        private readonly INetDaemonApp _defaultDaemonApp;
        internal DaemonHostTestBase()
        {
            _loggerMock = new LoggerMock();
            _defaultHassClientMock = HassClientMock.DefaultMock;
            _defaultDataRepositoryMock = new Mock<IDataRepository>();
            _defaultDaemonHost = new NetDaemonHost(_defaultHassClientMock.Object, _defaultDataRepositoryMock.Object, _loggerMock.LoggerFactory);
            _defaultDaemonApp = new BaseTestApp();
            _defaultDaemonApp.StartUpAsync(_defaultDaemonHost);

            _notConnectedDaemonHost = new NetDaemonHost(HassClientMock.MockConnectFalse.Object, _defaultDataRepositoryMock.Object, _loggerMock.LoggerFactory);
        }

        public Mock<IDataRepository> DefaultDataRepositoryMock => _defaultDataRepositoryMock;
        public NetDaemonHost DefaultDaemonHost => _defaultDaemonHost;
        public INetDaemonApp DefaultDaemonApp => _defaultDaemonApp;
        public NetDaemonHost NotConnectedDaemonHost => _notConnectedDaemonHost;

        public HassClientMock DefaultHassClientMock => _defaultHassClientMock;

        public LoggerMock LoggerMock => _loggerMock;

        public string HelloWorldData => "Hello world!";

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
    }
}