
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;

namespace NetDaemon.Tests.Performance;

// /// <summary>
// ///
// /// </summary>
// public sealed class PerfServer : IAsyncDisposable
// {
//     public const int RecieiveBufferSize = 1024 * 4;
//     public IHost HomeAssistantHost { get; }
//
//     public PerfServer()
//     {
//         HomeAssistantHost = CreateHostBuilder().Build() ?? throw new ApplicationException("Failed to create host");
//         HomeAssistantHost.Start();
//         var server = HomeAssistantHost.Services.GetRequiredService<IServer>();
//         var addressFeature = server.Features.Get<IServerAddressesFeature>() ?? throw new NullReferenceException();
//         foreach (var address in addressFeature.Addresses)
//         {
//             ServerPort = int.Parse(address.Split(':').Last(), CultureInfo.InvariantCulture);
//             break;
//         }
//     }
//
//     public int ServerPort { get; }
//
//     public async ValueTask DisposeAsync()
//     {
//         await Stop().ConfigureAwait(false);
//     }
//
//     /// <summary>
//     ///     Starts a websocket server in a generic host
//     /// </summary>
//     /// <returns>Returns a IHostBuilder instance</returns>
//     private static IHostBuilder CreateHostBuilder()
//     {
//         return Host.CreateDefaultBuilder()
//             .ConfigureServices(s =>
//             {
//                 s.AddHttpClient();
//                 s.Configure<HostOptions>(
//                     opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(30)
//                 );
//             })
//             .ConfigureWebHostDefaults(webBuilder =>
//             {
//                 webBuilder.UseUrls("http://127.0.0.1:0"); //"http://172.17.0.2:5001"
//                 webBuilder.UseStartup<PerfServerStartup>();
//             });
//     }
//
//
//     /// <summary>
//     ///     Stops the fake Home Assistant server
//     /// </summary>
//     private async Task Stop()
//     {
//         await HomeAssistantHost.StopAsync().ConfigureAwait(false);
//         await HomeAssistantHost.WaitForShutdownAsync().ConfigureAwait(false);
//     }
//
// }

/// <summary>
///     The class implementing the mock hass server
/// </summary>
public sealed class PerfServerStartup(ILogger<PerfServerStartup> logger) : IDisposable
{

    private readonly CancellationTokenSource _cancelSource = new();

    // Get the path to mock testdata

    private static int DefaultTimeOut => 5000;
    private const int RecieiveBufferSize = 1024 * 4;

    // For testing the API we just return a entity
    // private static async Task ProcessRequest(HttpContext context)
    // {
    //     var entityName = "test.entity";
    //     if (context.Request.Method == "POST")
    //         entityName = "test.post";
    //
    //     await context.Response.WriteAsJsonAsync(
    //         new HassEntity
    //         {
    //             EntityId = entityName,
    //             DeviceId = "ksakksk22kssk2",
    //             AreaId = "ssksks2ksk3k333kk",
    //             Name = "name"
    //         }
    //     ).ConfigureAwait(false);
    // }

    /// <summary>
    ///     Replaces the id of the result being sent by the id of the command received
    /// </summary>
    /// <param name="responseMessageFileName">Filename of the result</param>
    /// <param name="id">Id of the command</param>
    /// <param name="websocket">The websocket to send to</param>
    // private async Task ReplaceIdInResponseAndSendMsg(string responseMessageFileName, int id, WebSocket websocket)
    // {
    //     var msg =
    //         await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Integration", "Testdata",
    //             responseMessageFileName)).ConfigureAwait(false);
    //     // All testdata has id=3 so easy to replace it
    //     msg = msg.Replace("\"id\": 3", $"\"id\": {id}", StringComparison.Ordinal);
    //     var bytes = Encoding.UTF8.GetBytes(msg);
    //
    //     await websocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
    //         WebSocketMessageType.Text, true, _cancelSource.Token).ConfigureAwait(false);
    // }


    private readonly ConcurrentBag<int> _eventSubscriptions = [];

    public static readonly ConcurrentBag<InputBoolean> _inputBooleans = [];

    private  async Task SendWebsocketMessage(WebSocket ws, string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        await ws.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
            WebSocketMessageType.Text, true, _cancelSource.Token).ConfigureAwait(false);
    }

    /// <summary>
    ///     Process incoming websocket requests to simulate Home Assistant websocket API
    /// </summary>
    public async Task ProcessWebsocket(WebSocket webSocket)
    {
        logger.LogDebug("Processing websocket");
        // Buffer is set.
        var buffer = new byte[RecieiveBufferSize];

        try
        {
            // First send auth required to the client
            var authRequiredMessage = Messages.AuthRequired;
            await SendWebsocketMessage(webSocket, authRequiredMessage).ConfigureAwait(false);

            // Console.WriteLine($"SERVER: WebSocketState = {webSocket.State}, MessageType = {result.MessageType}");
            while (true)
            {
                // Wait for incoming messages
                var result =
                    await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancelSource.Token)
                        .ConfigureAwait(false);

                logger.LogInformation("Received message: {Message}", Encoding.UTF8.GetString(buffer, 0, result.Count));

                _cancelSource.Token.ThrowIfCancellationRequested();

                if (result.CloseStatus.HasValue && webSocket.State == WebSocketState.CloseReceived)
                {
                    logger.LogInformation("Close message received, closing websocket");
                    break;
                }

                var hassMessage =
                    JsonSerializer.Deserialize<HassMessage>(new ReadOnlySpan<byte>(buffer, 0, result.Count))
                    ?? throw new ApplicationException("Unexpected not able to deserialize");
                switch (hassMessage.Type)
                {
                    case "auth":
                        // Just auth anything
                        var authOkMessage = Messages.AuthOk;
                        await SendWebsocketMessage(webSocket, authOkMessage).ConfigureAwait(false);
                        break;
                    case "subscribe_events":
                        var resultMsg = Messages.ResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, resultMsg).ConfigureAwait(false);
                        break;
                    case "get_states":
                        var getStatesResultMsg = Messages.GetStatesResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, getStatesResultMsg).ConfigureAwait(false);
                        break;
                    case "call_service":
                        logger.LogInformation("Received call_service message {Service}", hassMessage);
                        break;
                    case "get_config":
                        var getConfigResultMsg = Messages.GetConfigResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, getConfigResultMsg).ConfigureAwait(false);
                        break;
                    case "config/area_registry/list":
                        var getAreasResultMsg = Messages.GetAreasResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, getAreasResultMsg).ConfigureAwait(false);
                        break;
                    case "config/label_registry/list":
                        var getLabelsResultMsg = Messages.GetLabelsResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, getLabelsResultMsg).ConfigureAwait(false);
                        break;
                    case "config/floor_registry/list":
                        var getFloorsResultMsg = Messages.GetFloorsResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, getFloorsResultMsg).ConfigureAwait(false);
                        break;
                    case "config/device_registry/list":
                        var devicesResultMsg = Messages.GetDevicesResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, devicesResultMsg).ConfigureAwait(false);
                        break;
                    case "config/entity_registry/list":
                        var entitiesResultMsg = Messages.GetEntitiesResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, entitiesResultMsg).ConfigureAwait(false);
                        break;
                    case "input_boolean/create":
                        var createInputBooleanCommand =
                            JsonSerializer.Deserialize<CreateInputBooleanCommand>(
                                new ReadOnlySpan<byte>(buffer, 0, result.Count))
                            ?? throw new ApplicationException("Unexpected not able to deserialize input boolean");
                        var inputBoolean = new InputBoolean { Id = createInputBooleanCommand.Name, Name = createInputBooleanCommand.Name };
                        logger.LogInformation("Creating input_boolean {InputBoolean}", inputBoolean);
                        _inputBooleans.Add(inputBoolean);
                        var inputBooleanCreateResultMsg = Messages.ResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, inputBooleanCreateResultMsg).ConfigureAwait(false);
                        break;
                    case "input_boolean/list":
                        var inputBooleans = _inputBooleans.ToArray();
                        var response = JsonSerializer.Serialize(inputBooleans);
                        logger.LogInformation("Sending input_boolean/list response {Response}", response);
                        await SendWebsocketMessage(webSocket, response).ConfigureAwait(false);
                        break;
                    default:
                        logger.LogWarning("Unknown message type {Type}", hassMessage.Type);
                        throw new ApplicationException($"Unknown message type {hassMessage.Type}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Cancelled operation");
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal", CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Failed to process websocket");
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
        logger.LogInformation("Websocket processing done");
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

    // private sealed class AuthMessage
    // {
    //     [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    //     [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    // }
    //
    // private sealed class SendCommandMessage
    // {
    //     [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
    //     [JsonPropertyName("id")] public int Id { get; set; } = 0;
    // }

    public void Dispose()
    {
        _cancelSource.Dispose();
    }
}
