namespace NetDaemon.Client.Internal.Net;

/// <summary>
///     The pipeline makes a transport layer on top of WebSocketClient.
///     This pipeline handles json serialization
/// </summary>
internal interface IWebSocketClientTransportPipeline : IAsyncDisposable
{
    /// <summary>
    ///     State of the underlying websocket
    /// </summary>
    WebSocketState WebSocketState { get; }

    /// <summary>
    ///     Gets next message from pipeline
    /// </summary>
    ValueTask<T> GetNextMessageAsync<T>(CancellationToken cancellationToken) where T : class;

    /// <summary>
    ///     Sends a message to the pipeline
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendMessageAsync<T>(T message, CancellationToken cancellationToken) where T : class;

    /// <summary>
    ///     Close the pipeline, it will also close the underlying websocket
    /// </summary>
    Task CloseAsync();
}