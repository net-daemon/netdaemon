using System.Collections.Concurrent;
using System.Globalization;

namespace NetDaemon.HassClient.Tests.Integration;

/// <summary>
///     The Home Assistant Mock class implements a fake Home Assistant server by
///     exposing the websocket api and fakes responses to requests.
/// </summary>
public sealed class HomeAssistantMock : IAsyncDisposable
{
    public const int RecieiveBufferSize = 1024 * 4;
    public IHost HomeAssistantHost { get; }

    public HomeAssistantMock()
    {
        HomeAssistantHost = CreateHostBuilder().Build() ?? throw new ApplicationException("Failed to create host");
        HomeAssistantHost.Start();
        var server = HomeAssistantHost.Services.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>() ?? throw new NullReferenceException();
        foreach (var address in addressFeature.Addresses)
        {
            ServerPort = int.Parse(address.Split(':').Last(), CultureInfo.InvariantCulture);
            break;
        }
    }

    public int ServerPort { get; }

    public async ValueTask DisposeAsync()
    {
        await Stop().ConfigureAwait(false);
    }

    /// <summary>
    ///     Starts a websocket server in a generic host
    /// </summary>
    /// <returns>Returns a IHostBuilder instance</returns>
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(s =>
            {
                s.AddHttpClient();
                s.Configure<HostOptions>(
                    opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(30)
                );
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("http://127.0.0.1:0"); //"http://172.17.0.2:5001"
                webBuilder.UseStartup<HassMockStartup>();
            });
    }


    /// <summary>
    ///     Stops the fake Home Assistant server
    /// </summary>
    private async Task Stop()
    {
        await HomeAssistantHost.StopAsync().ConfigureAwait(false);
        await HomeAssistantHost.WaitForShutdownAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     The class implementing the mock hass server
/// </summary>
public sealed class HassMockStartup : IHostedService, IDisposable
{
    private readonly byte[] _authOkMessage =
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Integration", "Testdata", "auth_ok.json"));

    private readonly CancellationTokenSource _cancelSource = new();

    // Get the path to mock testdata
    private readonly string _mockTestdataPath = Path.Combine(AppContext.BaseDirectory, "Integration", "Testdata");

    public HassMockStartup(IConfiguration configuration)
    {
    }

    private static int DefaultTimeOut => 5000;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cancelSource.CancelAsync();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment e)
    {
        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromSeconds(120)
        };
        app.UseWebSockets(webSocketOptions);
        app.Map("/api/websocket", builder =>
        {
            builder.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await ProcessWebsocket(webSocket);
                    return;
                }

                await next();
            });
        });
        app.UseRouting();

        app.UseEndpoints(
            builder =>
            {
                builder.Map("/api/devices",
                    async context => { await ProcessRequest(context).ConfigureAwait(false); }
                );
            });
    }

    // For testing the API we just return a entity
    private static async Task ProcessRequest(HttpContext context)
    {
        var entityName = "test.entity";
        if (context.Request.Method == "POST")
            entityName = "test.post";

        await context.Response.WriteAsJsonAsync(
            new HassEntity
            {
                EntityId = entityName,
                DeviceId = "ksakksk22kssk2",
                AreaId = "ssksks2ksk3k333kk",
                Name = "name"
            }
        ).ConfigureAwait(false);
    }

    /// <summary>
    ///     Replaces the id of the result being sent by the id of the command received
    /// </summary>
    /// <param name="responseMessageFileName">Filename of the result</param>
    /// <param name="id">Id of the command</param>
    /// <param name="websocket">The websocket to send to</param>
    private async Task ReplaceIdInResponseAndSendMsg(string responseMessageFileName, int id, WebSocket websocket)
    {
        var msg =
            await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Integration", "Testdata",
                responseMessageFileName)).ConfigureAwait(false);
        // All testdata has id=3 so easy to replace it
        msg = msg.Replace("\"id\": 3", $"\"id\": {id}", StringComparison.Ordinal);
        var bytes = Encoding.UTF8.GetBytes(msg);

        await websocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
            WebSocketMessageType.Text, true, _cancelSource.Token).ConfigureAwait(false);
    }


    private readonly ConcurrentBag<int> _eventSubscriptions = new();

    /// <summary>
    ///     Process incoming websocket requests to simulate Home Assistant websocket API
    /// </summary>
    private async Task ProcessWebsocket(WebSocket webSocket)
    {
        // Buffer is set.
        var buffer = new byte[HomeAssistantMock.RecieiveBufferSize];

        try
        {
            // First send auth required to the client
            var authRequiredMessage =
                await File.ReadAllBytesAsync(Path.Combine(_mockTestdataPath, "auth_required.json"))
                    .ConfigureAwait(false);

            await webSocket.SendAsync(new ArraySegment<byte>(authRequiredMessage, 0, authRequiredMessage.Length),
                WebSocketMessageType.Text, true, _cancelSource.Token).ConfigureAwait(false);


            // Console.WriteLine($"SERVER: WebSocketState = {webSocket.State}, MessageType = {result.MessageType}");
            while (true)
            {
                // Wait for incoming messages
                var result =
                    await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancelSource.Token)
                        .ConfigureAwait(false);

                _cancelSource.Token.ThrowIfCancellationRequested();

                if (result.CloseStatus.HasValue && webSocket.State == WebSocketState.CloseReceived)
                    break;

                var hassMessage =
                    JsonSerializer.Deserialize<HassMessage>(new ReadOnlySpan<byte>(buffer, 0, result.Count))
                    ?? throw new ApplicationException("Unexpected not able to deserialize");
                switch (hassMessage.Type)
                {
                    // We have an auth message
                    case "auth":
                        var authMessage =
                            JsonSerializer.Deserialize<AuthMessage>(
                                new ReadOnlySpan<byte>(buffer, 0, result.Count));
                        if (authMessage?.AccessToken == "ABCDEFGHIJKLMNOPQ")
                        {
                            // Hardcoded to be correct for test-case
                            // byte[] authOkMessage = File.ReadAllBytes (Path.Combine (this.mockTestdataPath, "auth_ok.json"));
                            await webSocket.SendAsync(
                                new ArraySegment<byte>(_authOkMessage, 0, _authOkMessage.Length),
                                WebSocketMessageType.Text, true, _cancelSource.Token).ConfigureAwait(false);
                        }
                        else
                        {
                            // Hardcoded to be correct for test-case
                            var authNotOkMessage =
                                await File.ReadAllBytesAsync(Path.Combine(_mockTestdataPath, "auth_notok.json"))
                                    .ConfigureAwait(false);
                            await webSocket.SendAsync(
                                new ArraySegment<byte>(authNotOkMessage, 0, authNotOkMessage.Length),
                                WebSocketMessageType.Text, true, _cancelSource.Token).ConfigureAwait(false);
                            // Hass will normally close session here but for the sake of testing the mock wont
                        }

                        break;
                    case "ping":
                        await ReplaceIdInResponseAndSendMsg(
                            "pong.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "subscribe_events":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_msg.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);

                        _eventSubscriptions.Add(hassMessage.Id);

                        // We wait so the subscription is added before we send the event
                        await Task.Delay(500).ConfigureAwait(false);
                        await ReplaceIdInResponseAndSendMsg(
                            "event.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "get_states":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_states.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "get_services":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_get_services.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "call_service":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_msg.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "execute_script":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_calendar_list_event.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "get_config":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_config.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "config/area_registry/list":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_get_areas.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);

                        break;
                    case "config/device_registry/list":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_get_devices.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "config/entity_registry/list":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_get_entities.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "fake_return_error":
                        await ReplaceIdInResponseAndSendMsg(
                            "result_msg_error.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);
                        break;
                    case "fake_service_event":
                        // Here we fake the server sending a service
                        // event by returning success and then
                        // return a service event
                        await ReplaceIdInResponseAndSendMsg(
                            "result_msg.json",
                            hassMessage.Id,
                            webSocket).ConfigureAwait(false);

                        foreach (var subscription in _eventSubscriptions)
                        {
                            await ReplaceIdInResponseAndSendMsg(
                                "service_event.json",
                                subscription,
                                webSocket).ConfigureAwait(false);
                        }
                        break;
                    case "fake_disconnect_test":
                        // This is not a real home assistant message, just used to test disconnect from socket.
                        // This one tests a normal disconnect
                        var timeout = new CancellationTokenSource(5000);
                        try
                        {
                            // Send close message (some bug n CloseAsync makes we have to do it this way)
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing",
                                timeout.Token).ConfigureAwait(false);
                            // Wait for close message
                            //await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), timeout.Token);
                        }
                        catch (OperationCanceledException)
                        {
                        }

                        return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal", CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw new ApplicationException("The thing is closed unexpectedly", e);
        }
        finally
        {
            try
            {
                await SendCorrectCloseFrameToRemoteWebSocket(webSocket).ConfigureAwait(false);
            }
            catch
            {
                // Just fail silently
            }
        }
    }

    /// <summary>
    ///     Closes correctly the websocket depending on websocket state
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Closing a websocket has special handling. When the client
    ///         wants to close it calls CloseAsync and the websocket takes
    ///         care of the proper close handling.
    ///     </para>
    ///     <para>
    ///         If the remote websocket wants to close the connection dotnet
    ///         implementation requires you to use CloseOutputAsync instead.
    ///     </para>
    ///     <para>
    ///         We do not want to cancel operations until we get closed state
    ///         this is why own timer cancellation token is used and we wait
    ///         for correct state before returning and disposing any connections
    ///     </para>
    /// </remarks>
    private static async Task SendCorrectCloseFrameToRemoteWebSocket(WebSocket ws)
    {
        using var timeout = new CancellationTokenSource(DefaultTimeOut);

        try
        {
            switch (ws.State)
            {
                case WebSocketState.CloseReceived:
                {
                    // after this, the socket state which change to CloseSent
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token)
                        .ConfigureAwait(false);
                    // now we wait for the server response, which will close the socket
                    while (ws.State != WebSocketState.Closed && !timeout.Token.IsCancellationRequested)
                        await Task.Delay(100).ConfigureAwait(false);
                    break;
                }
                case WebSocketState.Open:
                {
                    // Do full close
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token)
                        .ConfigureAwait(false);
                    if (ws.State != WebSocketState.Closed)
                        throw new ApplicationException("Expected the websocket to be closed!");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal upon task/token cancellation, disregard
        }
    }

    private sealed class AuthMessage
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class SendCommandMessage
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("id")] public int Id { get; set; } = 0;
    }

    public void Dispose()
    {
        _cancelSource.Dispose();
    }
}
