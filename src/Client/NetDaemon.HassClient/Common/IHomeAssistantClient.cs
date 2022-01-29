namespace NetDaemon.Client.Common;

/// <summary>
///     HomeAssistantClient
/// </summary>
public interface IHomeAssistantClient
{
    /// <summary>
    ///     Connect to Home Assistant
    /// </summary>
    /// <param name="host">The host name</param>
    /// <param name="port">Network port</param>
    /// <param name="ssl">Set true to use ssl</param>
    /// <param name="token">The access token to use</param>
    /// <param name="cancelToken">Cancellation token</param>
    Task<IHomeAssistantConnection> ConnectAsync(string host, int port, bool ssl, string token,
        CancellationToken cancelToken); 
    
    /// <summary>
    ///     Connect to Home Assistant
    /// </summary>
    /// <param name="host">The host name</param>
    /// <param name="port">Network port</param>
    /// <param name="ssl">Set true to use ssl</param>
    /// <param name="token">The access token to use</param>
    /// <param name="websocketPath">The relative websocket path to use connecting</param>
    /// <param name="cancelToken">Cancellation token</param>
    Task<IHomeAssistantConnection> ConnectAsync(string host, int port, bool ssl, string token, string websocketPath,
        CancellationToken cancelToken);
}
