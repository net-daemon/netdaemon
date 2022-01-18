namespace NetDaemon.Client.Internal.Net;

internal interface IWebSocketClient : IAsyncDisposable
{
    WebSocketState State { get; }
    WebSocketCloseStatus? CloseStatus { get; }

    Task ConnectAsync(Uri uri, CancellationToken cancel);

    Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription,
        CancellationToken cancellationToken);

    Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription,
        CancellationToken cancellationToken);

    Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
        CancellationToken cancellationToken);

    ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage,
        CancellationToken cancellationToken);

    ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
}