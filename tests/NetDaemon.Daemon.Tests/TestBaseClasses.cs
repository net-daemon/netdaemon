using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Model;
using Moq;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Fakes;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class BaseTestApp : NetDaemonRxApp { }

    public class BaseTestRxApp : NetDaemonRxApp { }

    public class MockedRxApp : NetDaemonRxApp
    {
        internal override IDisposable CreateObservableIntervall(TimeSpan timespan, Action action) => Mock.Of<IDisposable>();
    }

    public class CoreDaemonHostTestBase : DaemonHostTestBase, IAsyncLifetime, IDisposable
    {
        private readonly NetDaemonHost _notConnectedDaemonHost;
        private bool disposedValue;

        public CoreDaemonHostTestBase() : base()
        {
            DefaultDaemonApp = DefaultDaemonHost.LoadApp<BaseTestApp>("app_id");

            DefaultDaemonRxApp = DefaultDaemonHost.LoadApp<BaseTestRxApp>("app_rx_id");
            
            DefaultMockedRxApp = new Mock<NetDaemonRxApp>() { CallBase = true };
            DefaultMockedRxApp.Setup(n => n.CreateObservableIntervall(It.IsAny<TimeSpan>(), It.IsAny<Action>())).Returns(new Mock<IDisposable>().Object);

            DefaultServiceProviderMock.Services[typeof(NetDaemonRxApp)] = DefaultMockedRxApp.Object;
            DefaultDaemonHost.LoadApp<MockedRxApp>("app_rx_mock_id");

            var notConnectedHassClientFactoryMock = new HassClientFactoryMock(HassClientMock.MockConnectFalse);
            _notConnectedDaemonHost = new NetDaemonHost(notConnectedHassClientFactoryMock.Object, DefaultDataRepositoryMock.Object, LoggerMock.LoggerFactory);

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

            SetEntityState(new()
            {
                EntityId = "light.light_in_area",
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

            await _notConnectedDaemonHost.DisposeAsync().ConfigureAwait(false);
            await DefaultDaemonApp.DisposeAsync().ConfigureAwait(false);
            await DefaultDaemonRxApp.DisposeAsync().ConfigureAwait(false);
            await DefaultMockedRxApp.Object.DisposeAsync().ConfigureAwait(false);
            await DefaultDaemonRxApp.DisposeAsync().ConfigureAwait(false);
        }

        /// <summary>
        ///     Initializes on each test run
        /// </summary>
        public new async Task InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);

            await DefaultDaemonApp.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
            await DefaultDaemonRxApp.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
            await DefaultMockedRxApp.Object.StartUpAsync(DefaultDaemonHost).ConfigureAwait(false);
        }

        public BaseTestRxApp DefaultDaemonRxApp { get; }
        public Mock<NetDaemonRxApp> DefaultMockedRxApp { get; }
        public NetDaemonRxApp DefaultDaemonApp { get; }
        public static string HelloWorldData => "Hello world!";

        public (Task, CancellationTokenSource) ReturnRunningNotConnectedDaemonHostTask(short milliSeconds = 100, bool overrideDebugNotCancel = false)
        {
            var cancelSource = Debugger.IsAttached && !overrideDebugNotCancel
                ? new CancellationTokenSource()
                : new CancellationTokenSource(milliSeconds);
            return (_notConnectedDaemonHost.Run("host", 8123, false, "token", cancelSource.Token), cancelSource);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}