using System.Buffers;

namespace NetDaemon.Client.Internal.Net;

internal class WebSocketClientTransportPipeline(IWebSocketClient clientWebSocket) : IWebSocketClientTransportPipeline
{
    /// <summary>
    ///     Default Json serialization options, Hass expects intended
    /// </summary>
    private readonly JsonSerializerOptions _defaultSerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly CancellationTokenSource _internalCancelSource = new();
    private readonly IWebSocketClient _ws = clientWebSocket ?? throw new ArgumentNullException(nameof(clientWebSocket));

    private static int DefaultTimeOut => 5000;

    public WebSocketState WebSocketState => _ws.State;

    public async Task CloseAsync()
    {
        await SendCorrectCloseFrameToRemoteWebSocket().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            // In case we are just "disposing" without disconnect first
            // we call the close and fail silently if so
            await SendCorrectCloseFrameToRemoteWebSocket().ConfigureAwait(false);
        }
        catch
        {
            // Ignore all error in dispose
        }

        await _ws.DisposeAsync().ConfigureAwait(false);
        _internalCancelSource.Dispose();
    }

    public async ValueTask<T[]> GetNextMessagesAsync<T>(CancellationToken cancelToken) where T : class
    {
        if (_ws.State != WebSocketState.Open)
            throw new ApplicationException("Cannot send data on a closed socket!");

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _internalCancelSource.Token,
            cancelToken
        );
        var messageBuffer = new ArrayBufferWriter<byte>();
        await ReadMessageFromWebSocketAsync(messageBuffer, combinedTokenSource.Token).ConfigureAwait(false);
        combinedTokenSource.Token.ThrowIfCancellationRequested();
        var result = DeserializeMessages<T>(messageBuffer.WrittenSpan);
        // File.WriteAllText("./json_result.json", JsonSerializer.Serialize<T>(result, _defaultSerializerOptions));
        combinedTokenSource.Token.ThrowIfCancellationRequested();
        return result;
    }

    public Task SendMessageAsync<T>(T message, CancellationToken cancelToken) where T : class
    {
        if (cancelToken.IsCancellationRequested || _ws.State != WebSocketState.Open || _ws.CloseStatus.HasValue)
            throw new ApplicationException("Sending message on closed socket!");

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _internalCancelSource.Token,
            cancelToken
        );

        var result = JsonSerializer.SerializeToUtf8Bytes(message, message.GetType(),
            _defaultSerializerOptions);

        return _ws.SendAsync(result, WebSocketMessageType.Text, true, combinedTokenSource.Token);
    }

    private static T[] DeserializeMessages<T>(ReadOnlySpan<byte> payload)
        where T : class
    {
        if (payload.IsEmpty)
            throw new ApplicationException("Deserialization of websocket returned empty result (null)");

        if (IsJsonArray(payload))
            return JsonSerializer.Deserialize<T[]>(payload) ?? throw new ApplicationException(
                "Deserialization of websocket returned empty result (null)");

        var obj = JsonSerializer.Deserialize<T>(payload) ?? throw new ApplicationException(
            "Deserialization of websocket returned empty result (null)");
        return [obj];
    }

    private static bool IsJsonArray(ReadOnlySpan<byte> payload)
    {
        foreach (var value in payload)
        {
            if (value is (byte)' ' or (byte)'\n' or (byte)'\r' or (byte)'\t')
                continue;

            return value == (byte)'[';
        }

        throw new ApplicationException("Deserialization of websocket returned empty result (null)");
    }

    /// <summary>
    ///     Read one or more chunks of a message and writes the result
    ///     to the buffer
    /// </summary>
    /// <remarks>
    ///     A websocket message can be 1 to several chunks of data.
    ///     As data are read it is written to a contiguous buffer so we can inspect
    ///     the first token and deserialize directly to either a single message or
    ///     a coalesced message array.
    /// </remarks>
    private async Task ReadMessageFromWebSocketAsync(ArrayBufferWriter<byte> messageBuffer, CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested && !_ws.CloseStatus.HasValue)
        {
            var memory = messageBuffer.GetMemory();
            var result = await _ws.ReceiveAsync(memory, cancelToken).ConfigureAwait(false);
            if (
                _ws.State == WebSocketState.Open &&
                result.MessageType != WebSocketMessageType.Close)
            {
                messageBuffer.Advance(result.Count);

                if (result.EndOfMessage) break;
            }
            else if (_ws.State == WebSocketState.CloseReceived)
            {
                // We got a close message from server or if it still open we got canceled
                // in both cases it is important to send back the close message
                await SendCorrectCloseFrameToRemoteWebSocket().ConfigureAwait(false);

                // Cancel so the read operation is canceled before returning
                await _internalCancelSource.CancelAsync();
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
    private async Task SendCorrectCloseFrameToRemoteWebSocket()
    {
        using var timeout = new CancellationTokenSource(DefaultTimeOut);

        try
        {
            switch (_ws.State)
            {
                case WebSocketState.CloseReceived:
                {
                    // after this, the socket state which change to CloseSent
                    await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token)
                        .ConfigureAwait(false);
                    // now we wait for the server response, which will close the socket
                    while (_ws.State != WebSocketState.Closed && !timeout.Token.IsCancellationRequested)
                        await Task.Delay(100).ConfigureAwait(false);
                    break;
                }
                case WebSocketState.Open:
                {
                    // Do full close
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token)
                        .ConfigureAwait(false);
                    if (_ws.State != WebSocketState.Closed)
                        throw new ApplicationException("Expected the websocket to be closed!");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal upon task/token cancellation, disregard
        }
        finally
        {
            // After the websocket is properly closed
            // we can safely cancel all actions
            if (!_internalCancelSource.IsCancellationRequested)
                await _internalCancelSource.CancelAsync();
        }
    }
}
