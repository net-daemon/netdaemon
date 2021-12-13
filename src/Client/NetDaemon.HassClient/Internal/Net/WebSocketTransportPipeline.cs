namespace NetDaemon.Client.Internal.Net;

internal class WebSocketClientTransportPipeline : IWebSocketClientTransportPipeline
{
    private readonly IWebSocketClient _ws;
    private readonly Pipe _pipe = new();

    private static int DefaultTimeOut => 5000;

    public WebSocketState WebSocketState => _ws.State;

    private readonly CancellationTokenSource _internalCancelSource = new();

    public WebSocketClientTransportPipeline(IWebSocketClient clientWebSocket, ILogger<IWebSocketClientTransportPipeline> logger)
    {
        _ws = clientWebSocket ?? throw new ArgumentNullException(nameof(clientWebSocket));
    }

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
    }

    public async ValueTask<T> GetNextMessageAsync<T>(CancellationToken cancelToken) where T : class
    {
        if (_ws.State != WebSocketState.Open)
            throw new ApplicationException("Cannot send data on a closed socket!");

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _internalCancelSource.Token,
            cancelToken
        );
        try
        {
            // First we start the serialization task that will process
            // the pipeline for new data written from websocket input 
            // We want the processing to start before we read data
            // from the websocket so the pipeline is not getting full
            var serializeTask = ReadMessageFromPipelineAndSerializeAsync<T>(combinedTokenSource.Token);
            await ReadMessageFromWebSocketAndWriteToPipelineAsync(combinedTokenSource.Token).ConfigureAwait(false);
            var result = await serializeTask.ConfigureAwait(false);
            // File.WriteAllText("./json_result.json", JsonSerializer.Serialize<T>(result, _defaultSerializerOptions));
            // We need to make sure the serialize task is finished before we throw the exception
            combinedTokenSource.Token.ThrowIfCancellationRequested();
            return result;
        }
        finally
        {
            _pipe.Reset();
        }
    }

    /// <summary>
    ///     Continuously reads the data from the pipe and serialize to object
    ///     from the json that are read
    /// </summary>
    /// <param name="cancelToken">Cancellation token</param>
    /// <typeparam name="T">The type to serialize to</typeparam>
    private async ValueTask<T> ReadMessageFromPipelineAndSerializeAsync<T>(CancellationToken cancelToken)
    {
        try
        {
           var message = await JsonSerializer.DeserializeAsync<T>(_pipe.Reader.AsStream(),
                                cancellationToken: cancelToken).ConfigureAwait(false)
                                    ?? throw new ApplicationException("Deserialization of websocket returned empty result (null)");
            return message;
        }
        finally
        {
            // Always complete the reader
            await _pipe.Reader.CompleteAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Read one or more chunks of a message and writes the result
    ///     to the pipeline
    /// </summary>
    /// <remarks>
    ///     A websocket message can be 1 to several chunks of data.
    ///     As data are read it is written on the pipeline for
    ///     the json serializer in function ReadMessageFromPipelineAndSerializeAsync
    ///     to continuously serialize. Using pipes is very efficient
    ///     way to reuse memory and get speedy results
    /// </remarks>
    private async Task ReadMessageFromWebSocketAndWriteToPipelineAsync(CancellationToken cancelToken)
    {
        try
        {
            while (!cancelToken.IsCancellationRequested && !_ws.CloseStatus.HasValue)
            {
                var memory = _pipe.Writer.GetMemory();
                var result = await _ws.ReceiveAsync(memory, cancelToken).ConfigureAwait(false);
                if (
                    _ws.State == WebSocketState.Open &&
                    result.MessageType != WebSocketMessageType.Close)
                {
                    _pipe.Writer.Advance(result.Count);

                    await _pipe.Writer.FlushAsync(cancelToken).ConfigureAwait(false);

                    if (result.EndOfMessage)
                    {
                        break;
                    }
                }
                else if (_ws.State == WebSocketState.CloseReceived)
                {
                    // We got a close message from server or if it still open we got canceled
                    // in both cases it is important to send back the close message
                    await SendCorrectCloseFrameToRemoteWebSocket().ConfigureAwait(false);

                    // Cancel so the write thread is canceled before pipe is complete
                    _internalCancelSource.Cancel();
                }
            }
        }
        finally
        {
            // We have successfully read the whole message, 
            // make available to reader 
            // even if failure or we cannot reset the pipe
            await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        }
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

    /// <summary>
    ///     Closes correctly the websocket depending on websocket state
    /// </summary>
    /// <remarks>
    /// <para>
    ///     Closing a websocket has special handling. When the client
    ///     wants to close it calls CloseAsync and the websocket takes
    ///     care of the proper close handling.
    /// </para>
    /// <para>
    ///     If the remote websocket wants to close the connection dotnet
    ///     implementation requires you to use CloseOutputAsync instead.
    /// </para>
    /// <para>
    ///     We do not want to cancel operations until we get closed state
    ///     this is why own timer cancellation token is used and we wait
    ///     for correct state before returning and disposing any connections
    /// </para>
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
                    await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token).ConfigureAwait(false);
                    // now we wait for the server response, which will close the socket
                    while (_ws.State != WebSocketState.Closed && !timeout.Token.IsCancellationRequested)
                        await Task.Delay(100).ConfigureAwait(false);
                    break;
                }
                case WebSocketState.Open:
                {
                    // Do full close 
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token).ConfigureAwait(false);
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
                _internalCancelSource.Cancel();
        }
    }

    /// <summary>
    ///     Default Json serialization options, Hass expects intended
    /// </summary>
    private readonly JsonSerializerOptions _defaultSerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}