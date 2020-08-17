using System;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
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
using NetDaemon.Daemon.Tests.Daemon;
using NetDaemon.Service.Api;
using Xunit;
using System.Threading;
using System.Text.Unicode;
using System.Text;
using System.Net.WebSockets;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common.Configuration;

namespace NetDaemon.Daemon.Tests.DaemonRunner.Api
{
    public class ApiFakeStartup
    {
        private readonly Common.NetDaemonApp _defaultDaemonApp;
        private readonly Common.NetDaemonApp _defaultDaemonApp2;
        private readonly BaseTestRxApp _defaultDaemonRxApp;
        private readonly Mock<NetDaemonRxApp> _defaultMockedRxApp;
        private readonly NetDaemonHost _defaultDaemonHost;
        private readonly LoggerMock _loggerMock;

        private readonly Mock<IDataRepository> _defaultDataRepositoryMock;
        private readonly HassClientMock _defaultHassClientMock;
        private readonly HttpHandlerMock _defaultHttpHandlerMock;
        public IConfiguration Configuration { get; }

        public ApiFakeStartup(IConfiguration configuration)
        {
            Configuration = configuration;
            _loggerMock = new LoggerMock();
            _defaultHassClientMock = HassClientMock.DefaultMock;
            _defaultDataRepositoryMock = new Mock<IDataRepository>();
            _defaultHttpHandlerMock = new HttpHandlerMock();
            _defaultDaemonHost = new NetDaemonHost(
                _defaultHassClientMock.Object,
                _defaultDataRepositoryMock.Object,
                _loggerMock.LoggerFactory,
                _defaultHttpHandlerMock.Object);

            _defaultDaemonApp = new BaseTestApp();
            _defaultDaemonApp.Id = "app_id";
            _defaultDaemonApp.IsEnabled = true;
            _defaultDaemonHost.InternalRunningAppInstances[_defaultDaemonApp.Id!] = _defaultDaemonApp;
            _defaultDaemonHost.InternalAllAppInstances[_defaultDaemonApp.Id!] = _defaultDaemonApp;

            _defaultDaemonApp2 = new BaseTestApp();
            _defaultDaemonApp2.Id = "app_id2";
            _defaultDaemonApp2.RuntimeInfo.NextScheduledEvent = DateTime.Now;
            _defaultDaemonApp2.IsEnabled = false;
            _defaultDaemonHost.InternalRunningAppInstances[_defaultDaemonApp2.Id!] = _defaultDaemonApp2;
            _defaultDaemonHost.InternalAllAppInstances[_defaultDaemonApp2.Id!] = _defaultDaemonApp2;

            _defaultDaemonRxApp = new BaseTestRxApp();
            _defaultDaemonRxApp.Id = "app_rx_id";
            _defaultDaemonRxApp.IsEnabled = true;
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

            services.AddTransient<IHassClient>(n => _defaultHassClientMock.Object);
            services.AddTransient<IDataRepository>(n => _defaultDataRepositoryMock.Object);
            services.AddTransient<IHttpHandler, NetDaemon.Daemon.HttpHandler>();
            services.AddSingleton<NetDaemonHost>(n => _defaultDaemonHost);
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<ApiWebsocketMiddleware>();
        }
    }
    public class ApiTests : IAsyncLifetime
    {
        // protected readonly EventQueueManager EventQueueManager;
        private readonly TestServer _server;

        private readonly ArraySegment<byte> _buffer;

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ApiTests()
        {
            _buffer = new ArraySegment<byte>(new byte[8192]);

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

        private async Task<string> ReadString(WebSocket ws)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            _ = buffer.Array ?? throw new NullReferenceException("Failed to allocate memory buffer");

            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    throw new Exception("Unexpected type");
                }

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        private async Task<WebSocket> GetWsClient()
        {
            var wsc = _server.CreateWebSocketClient();
            return await wsc.ConnectAsync(new Uri(_server.BaseAddress, "ws"), CancellationToken.None).ConfigureAwait(false);
        }

        private async Task<object> ReadObject(WebSocket ws, Type t)
        {
            var s = await ReadString(ws);
            return JsonSerializer.Deserialize(s, t, _jsonOptions);
        }

        [Fact]
        public async Task TestGetApps()
        {

            var websocket = await GetWsClient();

            await websocket.SendAsync(Encoding.UTF8.GetBytes(@"{""type"": ""apps""}"), WebSocketMessageType.Text, true, CancellationToken.None);

            var res = (WsAppsResult)await ReadObject(websocket, typeof(WsAppsResult));
            var response = res.Data!;

            Assert.Equal(4, response?.Count());

            var app = response.Where(n => n.Id == "app_id").First();
            Assert.True(app.IsEnabled);
            Assert.Null(app.NextScheduledEvent);

            var app2 = response.Where(n => n.Id == "app_id2").First();
            Assert.False(app2.IsEnabled);
            // Should be null if disabled always
            Assert.Null(app.NextScheduledEvent);

            var appRx = response.Where(n => n.Id == "app_rx_id").First();
            Assert.True(appRx.IsEnabled);
            Assert.NotNull(appRx.NextScheduledEvent);
        }

        [Fact]
        public async Task TestGetConfig()
        {
            var websocket = await GetWsClient();

            await websocket.SendAsync(Encoding.UTF8.GetBytes(@"{""type"": ""settings""}"), WebSocketMessageType.Text, true, CancellationToken.None);

            var res = (WsConfigResult)await ReadObject(websocket, typeof(WsConfigResult));
            var response = res.Data;

            Assert.NotNull(response);

            Assert.NotNull(response?.DaemonSettings?.ProjectFolder);
            Assert.NotNull(response?.DaemonSettings?.SourceFolder);

        }
    }
}