namespace NetDaemon.Client.Common;

/// <summary>
///     Make connection to Home Assistant
/// </summary>
public static class HomeAssistantClientConnector
{
    /// <summary>
    ///     Connect to Home Assistant
    /// </summary>
    /// <param name="host">The host name</param>
    /// <param name="port">Network port</param>
    /// <param name="ssl">Set true to use ssl</param>
    /// <param name="token">The access token to use</param>
    /// <param name="cancelToken">Cancellation token</param>
    public static Task<IHomeAssistantConnection> ConnectClientAsync(string host, int port, bool ssl, string token,
        CancellationToken cancelToken)
    {
        return ConnectClientAsync(host, port, ssl, token, HomeAssistantSettings.DefaultWebSocketPath, cancelToken);
    }

    /// <summary>
    ///     Connect to Home Assistant
    /// </summary>
    /// <param name="host">The host name</param>
    /// <param name="port">Network port</param>
    /// <param name="ssl">Set true to use ssl</param>
    /// <param name="token">The access token to use</param>
    /// <param name="websocketPath">The relative websocket path to use connecting</param>
    /// <param name="cancelToken">Cancellation token</param>
    public static async Task<IHomeAssistantConnection> ConnectClientAsync(string host, int port, bool ssl, string token,
        string websocketPath,
        CancellationToken cancelToken)
    {
        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var loggerConnect = loggerFactory.CreateLogger<IHomeAssistantConnection>();
        var loggerClient = loggerFactory.CreateLogger<IHomeAssistantClient>();
        var settings = new HomeAssistantSettings
        {
            Host = host,
            Port = port,
            Ssl = ssl,
            Token = token
        };
        var optionsSettings = Options.Create(settings);
        var apiManager = new HomeAssistantApiManager(optionsSettings, new HttpClient());
        var connectionFactory = new HomeAssistantConnectionFactory(loggerConnect, apiManager);
        var pipeLineFactory = new WebSocketClientTransportPipelineFactory();
        var websocketClientFactory = new WebSocketClientFactory();
        var client = new HomeAssistantClient(loggerClient, websocketClientFactory, pipeLineFactory, connectionFactory);

        return await client.ConnectAsync(host, port, ssl, token, websocketPath, cancelToken).ConfigureAwait(false);
    }
}