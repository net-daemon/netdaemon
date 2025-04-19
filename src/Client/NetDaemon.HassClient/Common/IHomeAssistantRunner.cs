namespace NetDaemon.Client;

public interface IHomeAssistantRunner : IAsyncDisposable
{
    /// <summary>
    /// Observable that emits when a (new) connection is established.
    /// </summary>
    IObservable<IHomeAssistantConnection> OnConnect { get; }

    /// <summary>
    /// Observable that emits when the current connection is lost.
    /// </summary>
    IObservable<DisconnectReason> OnDisconnect { get; }

    /// <summary>
    /// The current connection to Home Assistant. Null if disconnected.
    /// </summary>
    IHomeAssistantConnection? CurrentConnection { get; }

    /// <summary>
    /// Maintains a connection to the Home Assistant server
    /// </summary>
    /// <param name="host">Host of Home Assistant instance</param>
    /// <param name="port">Port of Home Assistant instance</param>
    /// <param name="ssl">Use ssl</param>
    /// <param name="token">Home Assistant secret token</param>
    /// <param name="timeout">Wait time between connects</param>
    /// <param name="cancelToken">Cancel token</param>
    Task RunAsync(string host, int port, bool ssl, string token, TimeSpan timeout, CancellationToken cancelToken);
    
    /// <summary>
    /// Maintains a connection to the Home Assistant server
    /// </summary>
    /// <param name="host">Host of Home Assistant instance</param>
    /// <param name="port">Port of Home Assistant instance</param>
    /// <param name="ssl">Use ssl</param>
    /// <param name="token">Home Assistant secret token</param>
    /// <param name="websocketPath">The relative path to home assistant websocket endpoint</param>
    /// <param name="timeout">Wait time between connects</param>
    /// <param name="cancelToken">Cancel token</param>
    Task RunAsync(string host, int port, bool ssl, string token, string websocketPath, TimeSpan timeout, CancellationToken cancelToken);
}
