using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Fakes;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class BaseTestApp : Common.NetDaemonApp { }

    public class BaseTestRxApp : NetDaemonRxApp { }

    public class CoreDaemonHostTestBase : DaemonHostTestBase, IAsyncLifetime
    {
        private readonly Common.NetDaemonApp _defaultDaemonApp;
        private readonly BaseTestRxApp _defaultDaemonRxApp;
        private readonly Mock<NetDaemonRxApp> _defaultMockedRxApp;

        private readonly NetDaemonHost _notConnectedDaemonHost;

        public CoreDaemonHostTestBase() : base()
        {
            _defaultDaemonApp = new BaseTestApp();
            _defaultDaemonApp.Id = "app_id";
            _defaultDaemonApp.IsEnabled = true;

            DefaultDaemonHost.InternalRunningAppInstances[_defaultDaemonApp.Id!] = _defaultDaemonApp;

            _defaultDaemonRxApp = new BaseTestRxApp();
            _defaultDaemonRxApp.Id = "app_rx_id";
            _defaultDaemonRxApp.IsEnabled = true;
            DefaultDaemonHost.InternalRunningAppInstances[_defaultDaemonRxApp.Id!] = _defaultDaemonRxApp;

            _defaultMockedRxApp = new Mock<NetDaemonRxApp>() { CallBase = true };
            _defaultMockedRxApp.Object.Id = "app_rx_mock_id";
            _defaultMockedRxApp.Object.IsEnabled = true;
            _defaultMockedRxApp.Setup(n => n.CreateObservableIntervall(It.IsAny<TimeSpan>(), It.IsAny<Action>())).Returns(new Mock<IDisposable>().Object);
            DefaultDaemonHost.InternalRunningAppInstances[_defaultMockedRxApp.Object.Id!] = _defaultMockedRxApp.Object;

            _notConnectedDaemonHost = new NetDaemonHost(HassClientMock.MockConnectFalse.Object, DefaultDataRepositoryMock.Object, LoggerMock.LoggerFactory);

            SetupFakeData();
        }

        public void SetupFakeData()
        {
            SetEntityState(
            new HassState
            {
                EntityId = "light.correct_entity",
                Attributes = new Dictionary<string, object>
                {
                    ["test"] = 100
                },
                State = "on"
            });
            SetEntityState(
            new HassState
            {
                EntityId = "light.correct_entity2",
                Attributes = new Dictionary<string, object>
                {
                    ["test"] = 101
                },
                State = "off"
            });


            SetEntityState(new()
            {
                EntityId = "switch.correct_entity",
                Attributes = new Dictionary<string, object>
                {
                    ["test"] = 105
                }
            });

            SetEntityState(new()
            {
                EntityId = "light.filtered_entity",
                Attributes = new Dictionary<string, object>
                {
                    ["test"] = 90
                }
            });
            SetEntityState(new()
            {
                EntityId = "binary_sensor.pir",
                State = "off",
                Attributes = new Dictionary<string, object>
                {
                    ["device_class"] = "motion"
                }
            });

            SetEntityState(new()
            {
                EntityId = "binary_sensor.pir_2",
                State = "off",
                Attributes = new Dictionary<string, object>
                {
                    ["device_class"] = "motion"
                }
            });

            SetEntityState(new()
            {
                EntityId = "media_player.player",
                State = "off",
                Attributes = new Dictionary<string, object>
                {
                    ["anyattribute"] = "some attribute"
                }
            });

            SetEntityState( new()
            {
                EntityId = "light.ligth_in_area",
                State = "off",
                Attributes = new Dictionary<string, object>
                {
                    ["anyattribute"] = "some attribute"
                }
            });
        }

        /// <summary>
        ///     Clean-up any initialized test objects
        /// </summary>
        public new async Task DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);

            await _defaultDaemonApp.DisposeAsync().ConfigureAwait(false);
            await _defaultDaemonRxApp.DisposeAsync().ConfigureAwait(false);
            await _defaultMockedRxApp.Object.DisposeAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///     Initializes on each test run
        /// </summary>
        public new async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);

            await _defaultDaemonApp.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
            await _defaultDaemonRxApp.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
            await _defaultMockedRxApp.Object.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
        }

        public BaseTestRxApp DefaultDaemonRxApp => _defaultDaemonRxApp;
        public Mock<NetDaemonRxApp> DefaultMockedRxApp => _defaultMockedRxApp;
        public Common.NetDaemonApp DefaultDaemonApp => _defaultDaemonApp;
        public string HelloWorldData => "Hello world!";

        public (Task, CancellationTokenSource) ReturnRunningNotConnectedDaemonHostTask(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            var cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
            return (_notConnectedDaemonHost.Run("host", 8123, false, "token", cancelSource.Token), cancelSource);
        }


    }
}