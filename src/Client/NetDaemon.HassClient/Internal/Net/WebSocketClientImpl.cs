namespace NetDaemon.Client.Internal.Net;

/// <summary>
///     This class wraps the ClientWebSocket so we can do
///     mockable communications
/// </summary>
internal class WebSocketClientImpl : IWebSocketClient
{
    private readonly ClientWebSocket _ws;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="bypassCertificateErrors">
    ///     Provide the hash string for certificate that the websocket should ignore
    ///     errors from
    /// </param>
    public WebSocketClientImpl(bool bypassCertificateErrors = false)
    {
        _ws = new ClientWebSocket();

        if (bypassCertificateErrors)
            _ws.Options.RemoteCertificateValidationCallback = (_, cert, _, sslPolicyErrors) =>
            {
                return sslPolicyErrors == SslPolicyErrors.None || true;
            };
    }

    public WebSocketState State => _ws.State;

    public WebSocketCloseStatus? CloseStatus => _ws.CloseStatus;

    public Task ConnectAsync(Uri uri, CancellationToken cancel)
    {
        return _ws.ConnectAsync(uri, cancel);
    }

    public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription,
        CancellationToken cancellationToken)
    {
        return _ws.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }

    public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription,
        CancellationToken cancellationToken)
    {
        return _ws.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
        CancellationToken cancellationToken)
    {
        return _ws.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
    }

    public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType,
        bool endOfMessage, CancellationToken cancellationToken)
    {
        await Task.FromException(new NotImplementedException()).ConfigureAwait(false);
    }

    public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        return _ws.ReceiveAsync(buffer, cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        _ws.Dispose();
        return ValueTask.CompletedTask;
    }
}
