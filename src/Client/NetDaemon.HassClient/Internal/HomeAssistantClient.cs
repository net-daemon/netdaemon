namespace NetDaemon.Client.Internal;

internal class HomeAssistantClient : IHomeAssistantClient
{
    private readonly IHomeAssistantConnectionFactory _connectionFactory;
    private readonly ILogger<IHomeAssistantClient> _logger;
    private readonly IWebSocketClientTransportPipelineFactory _transportPipelineFactory;
    private readonly IWebSocketClientFactory _webSocketClientFactory;

    public HomeAssistantClient(
        ILogger<IHomeAssistantClient> logger,
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
        _logger.LogDebug("Connecting to Home Assistant websocket on {path}", websocketUri);
        var ws = _webSocketClientFactory.New();

        try
        {
            await ws.ConnectAsync(websocketUri, cancelToken).ConfigureAwait(false);

            var transportPipeline = _transportPipelineFactory.New(ws);

            var hassVersionInfo = await  HandleAuthorizationSequenceAndReturnHassVersionInfo(token, transportPipeline, cancelToken).ConfigureAwait(false);
            
            if (hassVersionInfo.Contains(".0b") 
            || Version.Parse(hassVersionInfo) >= new Version(2022, 9))
            {
                await AddCoalesceSupport(transportPipeline, cancelToken).ConfigureAwait(false);
            }

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

    private async Task AddCoalesceSupport(IWebSocketClientTransportPipeline transportPipeline, CancellationToken cancelToken)
    {
        var supportedFeaturesCommandMsg = new SupportedFeaturesCommand
            {Id = 1, Features = new Features() { CoalesceMessages = 1 }};
        
        // Send the supported features command
        await transportPipeline.SendMessageAsync(
            supportedFeaturesCommandMsg,
            cancelToken
        ).ConfigureAwait(false);
        
        // Get the result from command
        var resultMsg = await transportPipeline
            .GetNextMessagesAsync<HassMessage>(cancelToken).ConfigureAwait(false);

        if (resultMsg.Single().Success == true)
        {
            return;
        }
        throw new InvalidOperationException($"Failed to get result from supported feature command : {resultMsg.Single()}");
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

    private static async Task<string> HandleAuthorizationSequenceAndReturnHassVersionInfo(string token,
        IWebSocketClientTransportPipeline transportPipeline, CancellationToken cancelToken)
    {
        var connectTimeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        connectTimeoutTokenSource.CancelAfter(5000);
        // Begin the authorization sequence
        // Expect 'auth_required' 
        var msg = await transportPipeline.GetNextMessagesAsync<HassMessage>(connectTimeoutTokenSource.Token)
            .ConfigureAwait(false);
        if (msg[0].Type != "auth_required")
            throw new ApplicationException($"Unexpected type: '{msg[0].Type}' expected 'auth_required'");

        // Now send the auth message to Home Assistant
        await transportPipeline.SendMessageAsync(
            new HassAuthMessage {AccessToken = token},
            connectTimeoutTokenSource.Token
        ).ConfigureAwait(false);
        // Now get the result
        var authResultMessage = await transportPipeline
            .GetNextMessagesAsync<HassAuthResponse>(connectTimeoutTokenSource.Token).ConfigureAwait(false);

        switch (authResultMessage.Single().Type)
        {
            case "auth_ok":
                
                return authResultMessage[0].HaVersion;

            case "auth_invalid":
                await transportPipeline.CloseAsync().ConfigureAwait(false);
                throw new HomeAssistantConnectionException(DisconnectReason.Unauthorized);

            default:
                throw new ApplicationException($"Unexpected response ({authResultMessage.Single().Type})");
        }
    }
}
