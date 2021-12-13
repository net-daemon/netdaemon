namespace NetDaemon.Client.Internal.Net;

/// <summary>
///     This class wraps the ClientWebSocket so we can do 
///     mockable communications
/// </summary>
internal class WebSocketClientImpl : IWebSocketClient
{
    private readonly ClientWebSocket _ws;

    public WebSocketState State => _ws.State;

    public WebSocketCloseStatus? CloseStatus => _ws.CloseStatus;

    public Task ConnectAsync(Uri uri, CancellationToken cancel) => _ws.ConnectAsync(uri, cancel);

    public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription,
        CancellationToken cancellationToken) =>
        _ws.CloseAsync(closeStatus, statusDescription, cancellationToken);

    public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription,
        CancellationToken cancellationToken) =>
        _ws.CloseAsync(closeStatus, statusDescription, cancellationToken);

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
        CancellationToken cancellationToken) =>
        _ws.SendAsync(buffer, messageType, endOfMessage, cancellationToken);

    public async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType,
        bool endOfMessage, CancellationToken cancellationToken) =>
        await Task.FromException(new NotImplementedException()).ConfigureAwait(false);

    public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer,
        CancellationToken cancellationToken) =>
         Task.FromException<WebSocketReceiveResult>(new NotImplementedException());

    public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer,
        CancellationToken cancellationToken) => _ws.ReceiveAsync(buffer, cancellationToken);

    public ValueTask DisposeAsync()
    {
        _ws.Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="bypassCertificateErrorWithHash">Provide the hash string for certificate that the websocket should ignore errors from</param>
    public WebSocketClientImpl(string? bypassCertificateErrorWithHash = null)
    {
        _ws = new ClientWebSocket();

        if (string.IsNullOrEmpty(bypassCertificateErrorWithHash))
        {
            _ws.Options.RemoteCertificateValidationCallback = (_, cert, _, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;   //Is valid
                }

                return cert?.GetCertHashString() == bypassCertificateErrorWithHash?.ToUpperInvariant();
            };
        }
    }
}
