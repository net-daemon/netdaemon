using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Storage;
using NetDaemon.Service.Api;
using Xunit;
using System.Threading;
using System.Text;
using System.Net.WebSockets;
using System.IO;
using System.Text.Json;
using System.Linq;
using NetDaemon.Common.Configuration;
using NetDaemon.Daemon.Fakes;
using NetDaemon.Common.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace NetDaemon.Daemon.Tests.DaemonRunner.Api
{
    public class ApiFakeStartup : IAsyncLifetime, IDisposable
    {
        private readonly Mock<NetDaemonRxApp> _defaultMockedRxApp;
        private readonly Common.NetDaemonApp _defaultDaemonApp;
        private readonly Common.NetDaemonApp _defaultDaemonApp2;
        private readonly BaseTestRxApp _defaultDaemonRxApp;
        private readonly NetDaemonHost _defaultDaemonHost;
        private readonly HttpHandlerMock _defaultHttpHandlerMock;
        private readonly LoggerMock _loggerMock;

        private readonly Mock<IDataRepository> _defaultDataRepositoryMock;
        private readonly HassClientMock _defaultHassClientMock;
        private bool disposedValue;

        public IConfiguration Configuration { get; }

        public ApiFakeStartup(IConfiguration configuration)
        {
            Configuration = configuration;
            _loggerMock = new LoggerMock();
            _defaultHassClientMock = HassClientMock.DefaultMock;
            _defaultDataRepositoryMock = new Mock<IDataRepository>();
            _defaultHttpHandlerMock = new HttpHandlerMock();
            var hassClientFactoryMock = new HassClientFactoryMock(_defaultHassClientMock );
            _defaultDaemonHost = new NetDaemonHost(
                hassClientFactoryMock.Object,
                _defaultDataRepositoryMock.Object,
                _loggerMock.LoggerFactory,
                _defaultHttpHandlerMock.Object);

            _defaultDaemonApp = new BaseTestApp
            {
                Id = "app_id",
                IsEnabled = true
            };
            _defaultDaemonHost.InternalRunningAppInstances[_defaultDaemonApp.Id!] = _defaultDaemonApp;
            _defaultDaemonHost.InternalAllAppInstances[_defaultDaemonApp.Id!] = _defaultDaemonApp;

            _defaultDaemonApp2 = new BaseTestApp
            {
                Id = "app_id2"
            };
            _defaultDaemonApp2.RuntimeInfo.NextScheduledEvent = DateTime.Now;
            _defaultDaemonApp2.IsEnabled = false;
            _defaultDaemonHost.InternalRunningAppInstances[_defaultDaemonApp2.Id!] = _defaultDaemonApp2;
            _defaultDaemonHost.InternalAllAppInstances[_defaultDaemonApp2.Id!] = _defaultDaemonApp2;

            _defaultDaemonRxApp = new BaseTestRxApp
            {
                Id = "app_rx_id",
                IsEnabled = true
            };
            _defaultDaemonRxApp.RuntimeInfo.NextScheduledEvent = DateTime.Now;
            _defaultDaemonHost.InternalRunningAppInstances[_defaultDaemonRxApp.Id!] = _defaultDaemonRxApp;
            _defaultDaemonHost.InternalAllAppInstances[_defaultDaemonRxApp.Id!] = _defaultDaemonRxApp;

            _defaultMockedRxApp = new Mock<NetDaemonRxApp>() { CallBase = true };
            _defaultMockedRxApp.Object.Id = "app_rx_mock_id";
            _defaultMockedRxApp.Object.IsEnabled = true;
            _defaultMockedRxApp.Setup(n => n.CreateObservableIntervall(It.IsAny<TimeSpan>(), It.IsAny<Action>())).Returns(new Mock<IDisposable>().Object);
            _defaultDaemonHost.InternalRunningAppInstances[_defaultMockedRxApp.Object.Id!] = _defaultMockedRxApp.Object;
            _defaultDaemonHost.InternalAllAppInstances[_defaultMockedRxApp.Object.Id!] = _defaultMockedRxApp.Object;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<HomeAssistantSettings>(Configuration.GetSection("HomeAssistant"));
            services.Configure<NetDaemonSettings>(Configuration.GetSection("NetDaemon"));

            services.AddTransient(_ => _defaultHassClientMock.Object);
            services.AddTransient(_ => _defaultDataRepositoryMock.Object);
            services.AddTransient<IHttpHandler, HttpHandler>();
            services.AddSingleton(_ => _defaultDaemonHost);
            services.AddHttpClient();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120)
                // ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<ApiWebsocketMiddleware>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _defaultHttpHandlerMock.Dispose();
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

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _defaultDaemonApp.DisposeAsync().ConfigureAwait(false);
            await _defaultDaemonApp2.DisposeAsync().ConfigureAwait(false);
            await _defaultDaemonRxApp.DisposeAsync().ConfigureAwait(false);
            await _defaultDaemonHost.DisposeAsync().ConfigureAwait(false);
        }
    }

    public class ApiTests : IAsyncLifetime, IDisposable
    {
        // protected readonly EventQueueManager EventQueueManager;
        private readonly TestServer _server;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        private bool disposedValue;

        public ArraySegment<byte> Buffer { get; }

        public ApiTests()
        {
            Buffer = new ArraySegment<byte>(new byte[8192]);

            var builder = WebHost.CreateDefaultBuilder()
                .UseEnvironment("Testing")
                .UseStartup<ApiFakeStartup>();

            _server = new TestServer(builder);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        [SuppressMessage("", "CA2201")]
        private static async Task<string> ReadString(WebSocket ws)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            _ = buffer.Array ?? throw new NetDaemonNullReferenceException("Failed to allocate memory buffer");

            using var ms = new MemoryStream();
            WebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(buffer, CancellationToken.None).ConfigureAwait(false);
                ms.Write(buffer.Array, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            if (result.MessageType != WebSocketMessageType.Text)
            {
                throw new Exception("Unexpected type");
            }

            using var reader = new StreamReader(ms, Encoding.UTF8);
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        private async Task<WebSocket> GetWsClient()
        {
            var wsc = _server.CreateWebSocketClient();
            return await wsc.ConnectAsync(new Uri(_server.BaseAddress, "ws"), CancellationToken.None).ConfigureAwait(false);
        }

        private async Task<object?> ReadObject(WebSocket ws, Type t)
        {
            var s = await ReadString(ws).ConfigureAwait(false);
            return JsonSerializer.Deserialize(s, t, _jsonOptions);
        }

        [Fact]
        public async Task TestGetApps()
        {
            var websocket = await GetWsClient().ConfigureAwait(false);

            await websocket.SendAsync(Encoding.UTF8.GetBytes(@"{""type"": ""apps""}"), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);

            var res = (WsAppsResult?)await ReadObject(websocket, typeof(WsAppsResult)).ConfigureAwait(false);
            Assert.NotNull(res);
            var response = (res?.Data)!;

            Assert.Equal(4, response?.Count());

            var app = response?.Where(n => n.Id == "app_id").First();
            Assert.NotNull(app);
            Assert.True(app?.IsEnabled);
            Assert.Null(app?.NextScheduledEvent);

            var app2 = response?.Where(n => n.Id == "app_id2").First();
            Assert.NotNull(app2);
            Assert.False(app2?.IsEnabled);
            // Should be null if disabled always
            Assert.Null(app?.NextScheduledEvent);

            var appRx = response?.Where(n => n.Id == "app_rx_id").First();
            Assert.NotNull(appRx);
            Assert.True(appRx?.IsEnabled);
            Assert.NotNull(appRx?.NextScheduledEvent);
        }

        [Fact]
        public async Task TestGetConfig()
        {
            var websocket = await GetWsClient().ConfigureAwait(false);

            await websocket.SendAsync(Encoding.UTF8.GetBytes(@"{""type"": ""settings""}"), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);

            var res = (WsConfigResult?)await ReadObject(websocket, typeof(WsConfigResult)).ConfigureAwait(false);
            Assert.NotNull(res);
            var response = res?.Data;

            Assert.NotNull(response);

            Assert.NotNull(response?.DaemonSettings?.AppSource);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _server.Dispose();
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