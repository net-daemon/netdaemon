namespace NetDaemon.Client.Internal;

internal class HomeAssistantClient : IHomeAssistantClient
{
    private readonly IHomeAssistantConnectionFactory _connectionFactory;
    private readonly ILogger<HomeAssistantClient> _logger;
    private readonly IWebSocketClientTransportPipelineFactory _transportPipelineFactory;
    private readonly IWebSocketClientFactory _webSocketClientFactory;

    public HomeAssistantClient(
        ILogger<HomeAssistantClient> logger,
        IWebSocketClientFactory webSocketClientFactory,
        IWebSocketClientTransportPipelineFactory transportPipelineFactory,
        IHomeAssistantConnectionFactory connectionFactory
    )
    {
        _logger = logger;
        _webSocketClientFactory = webSocketClientFactory;
        _transportPipelineFactory = transportPipelineFactory;
        _connectionFactory = connectionFactory;
    }

    public Task<IHomeAssistantConnection> ConnectAsync(string host, int port, bool ssl, string token,
        CancellationToken cancelToken)
    {
        return ConnectAsync(host, port, ssl, token, HomeAssistantSettings.DefaultWebSocketPath, cancelToken);
    }

    public async Task<IHomeAssistantConnection> ConnectAsync(string host, int port, bool ssl, string token,
        string websocketPath,
        CancellationToken cancelToken)
    {
        var websocketUri = GetHomeAssistantWebSocketUri(host, port, ssl, websocketPath);
        _logger.LogInformation("Connecting to Home Assistant websocket on {path}", websocketUri);
        var ws = _webSocketClientFactory.New();

        try
        {
            await ws.ConnectAsync(websocketUri, cancelToken).ConfigureAwait(false);

            var transportPipeline = _transportPipelineFactory.New(ws);

            await HandleAuthorizationSequence(token, transportPipeline, cancelToken).ConfigureAwait(false);

            var connection = _connectionFactory.New(transportPipeline);

            if (await CheckIfRunning(connection, cancelToken).ConfigureAwait(false)) return connection;
            await connection.DisposeAsync().ConfigureAwait(false);
            throw new HomeAssistantConnectionException(DisconnectReason.NotReady);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Connect to Home Assistant was cancelled");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Error connecting to Home Assistant");
            throw;
        }
    }

    private static Uri GetHomeAssistantWebSocketUri(string host, int port, bool ssl, string websocketPath)
    {
        return new Uri($"{(ssl ? "wss" : "ws")}://{host}:{port}/{websocketPath}");
    }

    private static async Task<bool> CheckIfRunning(IHomeAssistantConnection connection, CancellationToken cancelToken)
    {
        var connectTimeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        connectTimeoutTokenSource.CancelAfter(5000);
        // Now send the auth message to Home Assistant
        var config = await connection
                         .SendCommandAndReturnResponseAsync<SimpleCommand, HassConfig>
                             (new SimpleCommand("get_config"), cancelToken).ConfigureAwait(false) ??
                     throw new NullReferenceException("Unexpected null return from command");

        return config.State == "RUNNING";
    }

    private static async Task HandleAuthorizationSequence(string token,
        IWebSocketClientTransportPipeline transportPipeline, CancellationToken cancelToken)
    {
        var connectTimeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        connectTimeoutTokenSource.CancelAfter(5000);
        // Begin the authorization sequence
        // Expect 'auth_required' 
        var msg = await transportPipeline.GetNextMessageAsync<HassMessage>(connectTimeoutTokenSource.Token)
            .ConfigureAwait(false);
        if (msg.Type != "auth_required")
            throw new ApplicationException($"Unexpected type: '{msg.Type}' expected 'auth_required'");

        // Now send the auth message to Home Assistant
        await transportPipeline.SendMessageAsync(
            new HassAuthMessage {AccessToken = token},
            connectTimeoutTokenSource.Token
        ).ConfigureAwait(false);
        // Now get the result
        var authResultMessage = await transportPipeline
            .GetNextMessageAsync<HassMessage>(connectTimeoutTokenSource.Token).ConfigureAwait(false);

        switch (authResultMessage.Type)
        {
            case "auth_ok":
                return;

            case "auth_invalid":
                await transportPipeline.CloseAsync().ConfigureAwait(false);
                throw new HomeAssistantConnectionException(DisconnectReason.Unauthorized);

            default:
                throw new ApplicationException($"Unexpected response ({authResultMessage.Type})");
        }
    }
}