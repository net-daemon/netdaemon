using System.Collections.Concurrent;

namespace NetDaemon.Tests.Performance;

/// <summary>
///     The class implementing the mock hass server
/// </summary>
public sealed class PerfServerStartup(ILogger<PerfServerStartup> logger) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cancelSource = new();

    private const int DefaultTimeOut = 5000;
    private const int RecieiveBufferSize = 1024 * 4;

    private readonly ConcurrentBag<int> _eventSubscriptions = [];
    private Task? _perfTestTask;

    public static readonly ConcurrentBag<InputBoolean> _inputBooleans = [];

    /// <summary>
    ///    Sends a websocket message to the client
    /// </summary>
    private  async Task SendWebsocketMessage(WebSocket ws, string message)
    {
        var byteMessage = Encoding.UTF8.GetBytes(message);
        await ws.SendAsync(new ArraySegment<byte>(byteMessage, 0, byteMessage.Length),
            WebSocketMessageType.Text, true, _cancelSource.Token).ConfigureAwait(false);
    }

    /// <summary>
    ///     Process incoming websocket requests to simulate Home Assistant websocket API
    /// </summary>
    /// <remarks>
    ///     This implements just enough of the HA websocket API to make NetDaemon happy
    /// </remarks>
    public async Task ProcessWebsocket(WebSocket webSocket)
    {
        logger.LogDebug("Processing websocket");

        var buffer = new byte[RecieiveBufferSize];

        try
        {
            // First send auth required to the client
            var authRequiredMessage = Messages.AuthRequiredMsg;
            await SendWebsocketMessage(webSocket, authRequiredMessage).ConfigureAwait(false);

            while (true)
            {
                // Wait for incoming messages
                var result =
                    await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancelSource.Token)
                        .ConfigureAwait(false);

                logger.LogDebug("Received message: {Message}", Encoding.UTF8.GetString(buffer, 0, result.Count));

                _cancelSource.Token.ThrowIfCancellationRequested();

                if (result.CloseStatus.HasValue && webSocket.State == WebSocketState.CloseReceived)
                {
                    logger.LogDebug("Close message received, closing websocket");
                    break;
                }

                var hassMessage =
                    JsonSerializer.Deserialize<HassMessage>(new ReadOnlySpan<byte>(buffer, 0, result.Count))
                    ?? throw new ApplicationException("Unexpected not able to deserialize to HassMessage");

                switch (hassMessage.Type)
                {
                    case "auth":
                        // Just auth ok anything
                        var authOkMessage = Messages.AuthOkMsg;
                        await SendWebsocketMessage(webSocket, authOkMessage).ConfigureAwait(false);
                        break;
                    case "subscribe_events":
                        _eventSubscriptions.Add(hassMessage.Id);
                        var resultMsg = Messages.ResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, resultMsg).ConfigureAwait(false);
                        break;
                    case "get_states":
                        var getStatesResultMsg = Messages.GetStatesResultMsg(hassMessage.Id);
                        await SendWebsocketMessage(webSocket, getStatesResultMsg).ConfigureAwait(false);
                        break;
                    case "call_service":
                        var callServiceMsg = Messages.ResultMsg(hassMessage.Id);
                        var callServiceCommand =
                            JsonSerializer.Deserialize<CallServiceCommand>(
                                new ReadOnlySpan<byte>(buffer, 0, result.Count))
                            ?? throw new ApplicationException("Unexpected not able to deserialize call service");

                        if (callServiceCommand.Service == "start_performance_test")
                        {
                            _perfTestTask = StartPerformanceTest(webSocket);
                        }
                        await SendWebsocketMessage(webSocket, callServiceMsg).ConfigureAwait(false);
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
                        var jsonInputBooleans = JsonSerializer.Serialize(inputBooleans);
                        var jsonDoc = JsonDocument.Parse(jsonInputBooleans);
                        var response = new HassMessage {
                            Id = hassMessage.Id,
                            Type = "result",
                            Success = true,
                            ResultElement = jsonDoc.RootElement };
                        var responseMsg = JsonSerializer.Serialize(response);
                        logger.LogInformation("Sending input_boolean/list response {Response}", response);
                        await SendWebsocketMessage(webSocket, responseMsg).ConfigureAwait(false);
                        break;
                    case "start_performance_test":
                        await SendWebsocketMessage(webSocket, Messages.ResultMsg(hassMessage.Id)).ConfigureAwait(false);
                        _perfTestTask = StartPerformanceTest(webSocket);
                        break;

                    default:
                        throw new ApplicationException($"Unknown message type {hassMessage.Type}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Cancelled operation");
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal", CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to process websocket");
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
    ///    Starts the performance test, sends 1 000 000 state changes as fast as possible
    ///    and then stops the performance test.
    ///    Todo: Make the number of sent messages configurable
    /// </summary>
    private async Task StartPerformanceTest(WebSocket webSocket)
    {
        // Make a small delay to make sure all websocket messages are processed
        // Not worth it to make something more fancy since this is not run in the CI
        await Task.Delay(1000).ConfigureAwait(false);

        var subscription = _eventSubscriptions.FirstOrDefault();
        if (subscription == 0)
        {
            logger.LogWarning("No subscriptions found, cannot start performance test");
            return;
        }

        logger.LogInformation("Starting performance test");

        var eventMessage = Messages.EventResultMsg(subscription, "on", "off");
        for (var i = 0; i < 1000000; i++)
        {
            await SendWebsocketMessage(webSocket, eventMessage).ConfigureAwait(false);
        }

        // Sends the last state change with state "stop" to make the client stop the performance test
        eventMessage = Messages.EventResultMsg(subscription, "on", "stop");
        await SendWebsocketMessage(webSocket, eventMessage).ConfigureAwait(false);
        logger.LogInformation("Performance test done");
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

    public async ValueTask DisposeAsync()
    {
        await _cancelSource.CancelAsync();
        if (_perfTestTask != null && !_perfTestTask.IsCompleted)
        {
            await _perfTestTask;
        }
        _cancelSource.Dispose();
    }
}
